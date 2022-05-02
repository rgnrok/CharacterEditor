using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Assets.Game.Scripts.Loaders;
using CharacterEditor.Helpers;
using CharacterEditor.Services;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;
using UnityEngine.U2D;
using UnityEngine.UI;

namespace CharacterEditor
{
    [Serializable]
    public class MaterialInfo
    {
        [SerializeField] internal TextureShaderType shader;
        [SerializeField] internal Material skinMaterial;
        [SerializeField] internal Material armorMeshMaterial;
        [SerializeField] internal Material faceMeshMaterial;
        [SerializeField] internal Material cloakMaterial;
    }

    public class TextureManager : MonoBehaviour
    {
        [SerializeField] private Material skinRenderShaderMaterial;
        [SerializeField] private RawImage skinRawImage;
        [SerializeField] private RenderTexture renderSkinTexture;
        [SerializeField] private Image portrait;
        [SerializeField] private SpriteAtlas portraits;

        [EnumFlag] public TextureType canChangeMask;
        private TextureType[] _canChangeTypes;

        [SerializeField] private MaterialInfo[] materials;
        public MaterialInfo[] Materials { get { return materials; } }

        public Dictionary<TextureType, CharacterTexture> CurrentCharacterTextures { get; private set; }
        public Texture2D CharacterTexture { get; private set; }
        public Texture2D CloakTexture { get; private set; }
        public bool IsReady { get; private set; }
        public Dictionary<string, TextureShaderType> CharacterShaders { get; private set; }
        public TextureShaderType CurrentCharacterShader { get; private set; }

        private List<SkinnedMeshRenderer> _modelRenderers;
        private List<SkinnedMeshRenderer> _cloakRenderers;
        private Dictionary<string, Dictionary<TextureType, CharacterTexture>> _characterTextures;

        private Dictionary<string, TwoWayArray<Sprite>> _characterPortraits;
        public Sprite CharacterPortrait { get { return _characterPortraits[_characterRace].Current; } }

        private string _characterRace;
        private TextureType[] _ignoreTypes;
        private bool _isLock;
        private ITextureLoader _textureLoader;
        private IDataManager _dataManager;

        public Action OnTexturesChanged;
        public Action OnTexturesLoaded;
        private MergeTextureService _mergeTextureService;

        public static TextureManager Instance { get; private set; }

        void Awake()
        {
            if (Instance != null) Destroy(gameObject);
            Instance = this;

            IsReady = false;

            _mergeTextureService = new MergeTextureService();

            _characterTextures = new Dictionary<string, Dictionary<TextureType, CharacterTexture>>();
            _modelRenderers = new List<SkinnedMeshRenderer>();
            _cloakRenderers = new List<SkinnedMeshRenderer>();
            CharacterShaders = new Dictionary<string, TextureShaderType>();

            CharacterTexture = new Texture2D(Constants.SKIN_TEXTURE_ATLAS_SIZE, Constants.SKIN_TEXTURE_ATLAS_SIZE, TextureFormat.RGB24, false);


            _canChangeTypes = canChangeMask.FlagToArray<TextureType>();



            _characterPortraits = new Dictionary<string, TwoWayArray<Sprite>>();
            var loaderService = AllServices.Container.Single<ILoaderService>();
            _textureLoader = loaderService.TextureLoader;
            _dataManager = loaderService.DataManager;
        }


        /*
         * Change Character. Update textures and skin meshes
         */
        public async Task ApplyConfig(CharacterConfig config, CharacterGameObjectData data)
        {
            _modelRenderers.Clear();
            _modelRenderers.AddRange(data.SkinMeshes);
            _modelRenderers.AddRange(data.ShortRobeMeshes);
            _modelRenderers.AddRange(data.LongRobeMeshes);

            _characterRace = config.folderName;
            if (!_characterTextures.ContainsKey(_characterRace))
            {
                _characterTextures[_characterRace] = new Dictionary<TextureType, CharacterTexture>(config.availableTextures.Length, EnumComparer.TextureType);
                foreach (var texture in config.availableTextures)
                {
                    if (Array.IndexOf(_canChangeTypes, texture) == -1) continue;
                    _characterTextures[_characterRace][texture] = TextureFactory.Create(texture, _textureLoader, _dataManager, _characterRace);
                }
            }

            CurrentCharacterTextures = _characterTextures[_characterRace];
            _cloakRenderers.Clear();
            _cloakRenderers.AddRange(data.CloakMeshes);

            await UpdateTextures();
            while (!IsReady) await Task.Yield();
           
            if (!CharacterShaders.ContainsKey(_characterRace) ||
                CharacterShaders[_characterRace] != CurrentCharacterShader)
                SetShader(CurrentCharacterShader);

            if (portraits != null)
            {
                if (!_characterPortraits.ContainsKey(_characterRace))
                {
                    var sprites = new Sprite[portraits.spriteCount];
                    portraits.GetSprites(sprites);
                    _characterPortraits[_characterRace] = new TwoWayArray<Sprite>(sprites);
                }

                portrait.sprite = _characterPortraits[_characterRace].Current;
            }
        }

        public MaterialInfo GetShaderMaterial(TextureShaderType shader)
        {
            foreach (var mat in materials)
                if (mat.shader == shader) return mat;

            return null;
        }

        public MaterialInfo GetShaderMaterial()
        {
            return GetShaderMaterial(CurrentCharacterShader);
        }

        /*
        * Update skin mesh materials and shaders
        */
        public void SetShader(TextureShaderType shader)
        {
            var materialInfo = GetShaderMaterial(shader);
            if (materialInfo == null)
                return;

            CurrentCharacterShader = shader;
            CharacterShaders[_characterRace] = CurrentCharacterShader;

            var material = materialInfo.skinMaterial;
            material.mainTexture = CharacterTexture;
            foreach (var render in _modelRenderers)
                render.material = material;

            var cloakMaterial = materialInfo.cloakMaterial;
            cloakMaterial.mainTexture = CloakTexture;
            foreach (var render in _cloakRenderers)
                render.material = cloakMaterial;
        }

        public void LockUpdate(bool isLock)
        {
            _isLock = isLock;
            UpdateModelTextures();
        }

        private async Task UpdateTextures()
        {
            IsReady = false;
            foreach (var texture in CurrentCharacterTextures.Values)
                while (!texture.IsReady) await Task.Yield();

            if (OnTexturesLoaded != null) OnTexturesLoaded();
            await MergeTextures();
            // yield return StartCoroutine(UpdateCloakTexture());
        }

   
        /*
         * Combining the texture of the character
         */
        private async Task MergeTextures()
        {
            IsReady = false;
            renderSkinTexture = await _mergeTextureService.MergeTextures(skinRenderShaderMaterial, renderSkinTexture, CurrentCharacterTextures, _ignoreTypes);

            RenderTexture.active = renderSkinTexture;
            Profiler.BeginSample("===== CharacterTexture.ReadPixels");


            // Graphics.CopyTexture(renderSkinTexture, 0, 0, 0, 0, renderSkinTexture.width, renderSkinTexture.height, CharacterTexture, 0,0, 0, 0);
            // Graphics.CopyTexture(renderSkinTexture,  CharacterTexture);


            CharacterTexture.ReadPixels(new Rect(0, 0, renderSkinTexture.width, renderSkinTexture.height), 0, 0);
            // CharacterTexture.Apply();
            Profiler.EndSample();
            UpdateModelTextures();

            IsReady = true;
            _ignoreTypes = null;
            OnTexturesChanged?.Invoke();
        }


        private void UpdateModelTextures()
        {
            if (_isLock) return;
            CharacterTexture.Apply();
            // foreach (var render in _modelRenderers)
            //     render.material.mainTexture = CharacterTexture;
            
            if (skinRawImage != null)
                skinRawImage.texture = CharacterTexture;
        }

        private IEnumerator UpdateCloakTexture()
        {
            CloakTexture = null;
            CharacterTexture cloak;
            if (!CurrentCharacterTextures.TryGetValue(TextureType.Cloak, out cloak)) yield break;

            while (!cloak.IsReady) yield return null;
            CloakTexture = cloak.Current;

            foreach (var render in _cloakRenderers)
                render.material.mainTexture = CloakTexture;
        }
        
        public void UpdateMaterial(Material mat)
        {
            foreach (var render in _modelRenderers)
                render.material = mat;
        }

        public void UpdateCloakMaterial(Material mat)
        {
            foreach (var render in _cloakRenderers)
                render.material = mat;
        }

        public void OnPrevTexture(TextureType[] types, TextureType[] clearTypes = null)
        {
            if (!IsReady) return;

            CharacterTexture mainTexture = null;
            foreach (var type in types)
            {
                if (!CurrentCharacterTextures.ContainsKey(type))
                    continue;

                if (mainTexture == null)
                {
                    mainTexture = CurrentCharacterTextures[type];
                    mainTexture.MovePrev();
                }
                else
                {
                    CurrentCharacterTextures[type].SetTexture(mainTexture.SelectedTexture);
                }
            }

            ResetTexture(clearTypes);
            OnChangeTexture(types);
        }

        public void OnPrevColor(TextureType[] types)
        {
            if (!IsReady) return;

            CharacterTexture mainTexture = null;
            foreach (var type in types)
            {
                if (!CurrentCharacterTextures.ContainsKey(type))
                    continue;

                if (mainTexture == null)
                {
                    mainTexture = CurrentCharacterTextures[type];
                    mainTexture.MovePrevColor();
                }
                else
                {
                    CurrentCharacterTextures[type].SetColor(mainTexture.SelectedColor);
                }
            }

            OnChangeTexture(types);
        }

        public void OnNextTexture(TextureType[] types, TextureType[] clearTypes = null)
        {
            if (!IsReady) return;

            CharacterTexture mainTexture = null;
            foreach (var type in types)
            {
                if (!CurrentCharacterTextures.ContainsKey(type))
                    continue;

                if (mainTexture == null)
                {
                    mainTexture = CurrentCharacterTextures[type];
                    mainTexture.MoveNext();
                }
                else
                {
                    CurrentCharacterTextures[type].SetTexture(mainTexture.SelectedTexture);
                }
            }

            ResetTexture(clearTypes);
            OnChangeTexture(types);
        }

        public void OnNextColor(TextureType[] types)
        {
            if (!IsReady) return;

            CharacterTexture mainTexture = null;
            foreach (var type in types)
            {
                if (!CurrentCharacterTextures.ContainsKey(type))
                    continue;

                if (mainTexture == null)
                {
                    mainTexture = CurrentCharacterTextures[type];
                    mainTexture.MoveNextColor();
                }
                else
                {
                    CurrentCharacterTextures[type].SetColor(mainTexture.SelectedColor);
                }
            }

            OnChangeTexture(types);
        }

        public void OnResetTexure(TextureType[] types)
        {
            if (!IsReady) return;
            ResetTexture(types);
            OnChangeTexture(types);
        }

        private void ResetTexture(TextureType[] types)
        {
            if (types == null)
                return;

            foreach (var type in types)
            {
                if (!CurrentCharacterTextures.ContainsKey(type))
                    continue;

                CurrentCharacterTextures[type].Reset();
            }
        }

        public void OnResetColor(TextureType[] types)
        {
            if (!IsReady) return;
            foreach (var type in types)
            {
                if (!CurrentCharacterTextures.ContainsKey(type))
                    continue;

                CurrentCharacterTextures[type].ResetColor();
            }

            OnChangeTexture(types);
        }

        public void OnClear(TextureType[] types)
        {
            if (!IsReady) return;

            foreach (var type in types)
            {
                if (!CurrentCharacterTextures.ContainsKey(type))
                    continue;

                CurrentCharacterTextures[type].ResetColor();
                CurrentCharacterTextures[type].Reset();
            }

            OnChangeTexture(types);
        }

        public void OnRandom(TextureType[] types, TextureType[] sameColors, TextureType[] ignoreTypes = null)
        {
            if (!IsReady) return;

            // Shuffle without same colors
            foreach (var type in types)
            {
                if (!CurrentCharacterTextures.ContainsKey(type) || Array.IndexOf(sameColors, type) != -1)
                    continue;

                CurrentCharacterTextures[type].Shuffle(true);
            }

            var color = -1;
            foreach (var sameColorType in sameColors)
            {
                if (!CurrentCharacterTextures.ContainsKey(sameColorType))
                    continue;

                if (color == -1)
                {
                    CurrentCharacterTextures[sameColorType].Shuffle(true);
                    color = CurrentCharacterTextures[sameColorType].SelectedColor;
                    continue;
                }

                CurrentCharacterTextures[sameColorType].ShuffleWithColor(color);
            }

            _ignoreTypes = ignoreTypes;
            OnChangeTexture(types);
        }

        /*
         * Prepare skin meshes and check merge texture
         */
        private async Task OnChangeTexture(TextureType[] changedTypes)
        {
            var types = new List<TextureType>();
            foreach (var type in changedTypes)
            {
                if (CurrentCharacterTextures.ContainsKey(type))
                    types.Add(type);
            }

            if (types.Count == 0) return;
            PrepareSkinMeshTextures(types);

            await UpdateTextures();
        }

        private void PrepareSkinMeshTextures(List<TextureType> types)
        {
            if (_ignoreTypes == null)
                return;

            var skined = new List<TextureType>()
            {
                TextureType.RobeLong,
                TextureType.RobeShort,
                TextureType.Pants
            };
            foreach (var ignore in _ignoreTypes)
            {
                skined.Remove(ignore);
                CurrentCharacterTextures[ignore].Reset();
            }

            if (skined.Count != 0 && CurrentCharacterTextures[skined[0]].SelectedTexture == 0)
                CurrentCharacterTextures[skined[0]].MoveNext();
        }

        public void OnPrevPortrait()
        {
            portrait.sprite = _characterPortraits[_characterRace].Prev;
        }

        public void OnNextPortrait()
        {
            portrait.sprite = _characterPortraits[_characterRace].Next;
        }
    }
}