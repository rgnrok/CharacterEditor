using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CharacterEditor.Helpers;
using CharacterEditor.Services;
using UnityEngine;
using UnityEngine.UI;

namespace CharacterEditor
{
    public class TextureManager : MonoBehaviour
    {
        [SerializeField] private Material skinRenderShaderMaterial;
        [SerializeField] private RawImage skinRawImage;
        [SerializeField] private RenderTexture renderSkinTexture;
        [SerializeField] private Image portrait;

        [EnumFlag] public TextureType canChangeMask;
        private TextureType[] _canChangeTypes;

        public static TextureManager Instance { get; private set; }

        public Texture2D CharacterTexture { get; private set; }
        public Texture2D CloakTexture { get; private set; }

        private TwoWayArray<Sprite> _currentCharacterPortrait;
        public Sprite CurrentCharacterPortrait => _currentCharacterPortrait.Current;
        public bool IsReady { get; private set; }

        private ILoaderService _loaderService;
        private IMergeTextureService _mergeTextureService;
        private IConfigManager _configManager;

        private Material _tmpSkinRenderMaterial;
        private List<Renderer> _modelRenderers;
        private List<Renderer> _cloakRenderers;

        private Dictionary<string, Dictionary<TextureType, CharacterTexture>> _characterTextures;
        private Dictionary<TextureType, CharacterTexture> _currentCharacterTextures;
        private Dictionary<string, TwoWayArray<Sprite>> _characterPortraits;

        private bool _isLock;
        private TextureType[] _ignoreTypes;

        public event Action OnTexturesChanged;
        public event Action OnTexturesUpdated;


        private void Awake()
        {
            if (Instance != null) Destroy(gameObject);
            Instance = this;

            _characterTextures = new Dictionary<string, Dictionary<TextureType, CharacterTexture>>();
            _modelRenderers = new List<Renderer>();
            _cloakRenderers = new List<Renderer>();

            _characterPortraits = new Dictionary<string, TwoWayArray<Sprite>>();

            CharacterTexture = new Texture2D(Constants.SKIN_TEXTURE_ATLAS_SIZE, Constants.SKIN_TEXTURE_ATLAS_SIZE, TextureFormat.RGB24, false);
            _tmpSkinRenderMaterial = new Material(skinRenderShaderMaterial);

            _canChangeTypes = canChangeMask.FlagToArray<TextureType>();

            _mergeTextureService = AllServices.Container.Single<IMergeTextureService>();
            _loaderService = AllServices.Container.Single<ILoaderService>();

            _configManager = AllServices.Container.Single<IConfigManager>();
            _configManager.OnChangeConfig += OnChangeConfigHandler;
        }

        private void OnDestroy()
        {
            if (_configManager != null)
                _configManager.OnChangeConfig -= OnChangeConfigHandler;
        }

        private async Task OnChangeConfigHandler(CharacterGameObjectData data)
        {
            await ApplyConfig(data);
        }

        private async Task ApplyConfig(CharacterGameObjectData data)
        {
            _modelRenderers.Clear();
            _modelRenderers.AddRange(data.SkinMeshes);
            _modelRenderers.AddRange(data.ShortRobeMeshes);
            _modelRenderers.AddRange(data.LongRobeMeshes);

            _cloakRenderers.Clear();
            _cloakRenderers.AddRange(data.CloakMeshes);

            var characterKey = data.Config.folderName;
            if (!_characterTextures.ContainsKey(characterKey))
            {
                _characterTextures[characterKey] = new Dictionary<TextureType, CharacterTexture>(data.Config.availableTextures.Length, EnumComparer.TextureType);
                foreach (var texture in data.Config.availableTextures)
                {
                    if (Array.IndexOf(_canChangeTypes, texture) == -1) continue;
                    _characterTextures[characterKey][texture] = TextureFactory.Create(texture, _loaderService.TextureLoader, _loaderService.DataManager, data.Config);
                }
            }
            _currentCharacterTextures = _characterTextures[characterKey];
 
            await UpdateTextures();
            await SetupPortrait(characterKey);
        }

        private async Task SetupPortrait(string characterKey)
        {
            if (!_characterPortraits.TryGetValue(characterKey, out _currentCharacterPortrait))
            {
                var portraits = await _loaderService.SpriteLoader.LoadPortraits();

                var sprites = new Sprite[portraits.spriteCount];
                portraits.GetSprites(sprites);
                _characterPortraits[characterKey] = _currentCharacterPortrait = new TwoWayArray<Sprite>(sprites);
            }

            portrait.sprite = _currentCharacterPortrait.Current;
        }

        public void LockUpdate(bool isLock)
        {
            if (_isLock == isLock) return;

            _isLock = isLock;
            if (!isLock) ApplyTextures();
        }

        #region OnXXXTexure actions
        public void OnPrevTexture(TextureType[] types, TextureType[] clearTypes = null)
        {
            if (!IsReady) return;

            CharacterTexture mainTexture = null;
            foreach (var type in types)
            {
                if (!_currentCharacterTextures.ContainsKey(type))
                    continue;

                if (mainTexture == null)
                {
                    mainTexture = _currentCharacterTextures[type];
                    mainTexture.MovePrev();
                }
                else
                {
                    _currentCharacterTextures[type].SetTexture(mainTexture.SelectedTexture);
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
                if (!_currentCharacterTextures.ContainsKey(type))
                    continue;

                if (mainTexture == null)
                {
                    mainTexture = _currentCharacterTextures[type];
                    mainTexture.MovePrevColor();
                }
                else
                {
                    _currentCharacterTextures[type].SetColor(mainTexture.SelectedColor);
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
                if (!_currentCharacterTextures.ContainsKey(type))
                    continue;

                if (mainTexture == null)
                {
                    mainTexture = _currentCharacterTextures[type];
                    mainTexture.MoveNext();
                }
                else
                {
                    _currentCharacterTextures[type].SetTexture(mainTexture.SelectedTexture);
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
                if (!_currentCharacterTextures.ContainsKey(type))
                    continue;

                if (mainTexture == null)
                {
                    mainTexture = _currentCharacterTextures[type];
                    mainTexture.MoveNextColor();
                }
                else
                {
                    _currentCharacterTextures[type].SetColor(mainTexture.SelectedColor);
                }
            }

            OnChangeTexture(types);
        }

        public void OnResetTexture(TextureType[] types)
        {
            if (!IsReady) return;
            ResetTexture(types);
            OnChangeTexture(types);
        }

        public void OnResetColor(TextureType[] types)
        {
            if (!IsReady) return;
            foreach (var type in types)
            {
                if (!_currentCharacterTextures.ContainsKey(type))
                    continue;

                _currentCharacterTextures[type].ResetColor();
            }

            OnChangeTexture(types);
        }

        public void OnClear(TextureType[] types)
        {
            if (!IsReady) return;

            foreach (var type in types)
            {
                if (!_currentCharacterTextures.ContainsKey(type))
                    continue;

                _currentCharacterTextures[type].ResetColor();
                _currentCharacterTextures[type].Reset();
            }

            OnChangeTexture(types);
        }

        public void OnRandom(TextureType[] types, TextureType[] sameColors, TextureType[] ignoreTypes = null)
        {
            if (!IsReady) return;

            // Shuffle without same colors
            foreach (var type in types)
            {
                if (!_currentCharacterTextures.ContainsKey(type) || Array.IndexOf(sameColors, type) != -1)
                    continue;

                _currentCharacterTextures[type].Shuffle(true);
            }

            var color = -1;
            foreach (var sameColorType in sameColors)
            {
                if (!_currentCharacterTextures.ContainsKey(sameColorType))
                    continue;

                if (color == -1)
                {
                    _currentCharacterTextures[sameColorType].Shuffle(true);
                    color = _currentCharacterTextures[sameColorType].SelectedColor;
                    continue;
                }

                _currentCharacterTextures[sameColorType].ShuffleWithColor(color);
            }

            OnChangeTexture(types, ignoreTypes);
        }
        #endregion

        private async Task UpdateTextures()
        {
            IsReady = false;

            foreach (var texture in _currentCharacterTextures.Values)
                while (!texture.IsReady) await Task.Yield();

            MergeTextures();
            UpdateCloakTexture();

            OnTexturesChanged?.Invoke();

            ApplyTextures();
            IsReady = true;
        }

        private void MergeTextures()
        {
            var mergeTextures = new Dictionary<string, Texture2D>();
            foreach (var texture in _currentCharacterTextures.Values)
            {
                var textureName = texture.GetShaderTextureName();
                if (textureName == null) continue;

                var isIgnoredType = _ignoreTypes != null && Array.IndexOf(_ignoreTypes, texture.Type) != -1;
                if (isIgnoredType) continue;

                mergeTextures[textureName] = texture.Current;
            }

            _mergeTextureService.MergeTextures(_tmpSkinRenderMaterial, renderSkinTexture, mergeTextures);

            RenderTexture.active = renderSkinTexture;
            CharacterTexture.ReadPixels(new Rect(0, 0, renderSkinTexture.width, renderSkinTexture.height), 0, 0);

            _ignoreTypes = null;
        }

        private void ApplyTextures()
        {
            if (_isLock) return;

            CharacterTexture.Apply();
            foreach (var render in _modelRenderers)
                render.material.mainTexture = CharacterTexture;

            if (skinRawImage != null)
                skinRawImage.texture = CharacterTexture;

            OnTexturesUpdated?.Invoke();
        }

        private void UpdateCloakTexture()
        {
            if (!_currentCharacterTextures.TryGetValue(TextureType.Cloak, out var cloak))
            {
                CloakTexture = null;
                return;
            }

            CloakTexture = cloak.Current;

            foreach (var render in _cloakRenderers)
                render.material.mainTexture = CloakTexture;
        }

        private async void OnChangeTexture(TextureType[] changedTypes, TextureType[] ignoreTypes = null)
        {
            var hasChangedTexture = changedTypes.Any(type => _currentCharacterTextures.ContainsKey(type));
            if (!hasChangedTexture) return;

            _ignoreTypes = ignoreTypes;
            PrepareSkinMeshTextures(changedTypes, ignoreTypes);
            await UpdateTextures();
        }

        private void PrepareSkinMeshTextures(TextureType[] types, TextureType[] ignoreTypes)
        {
            var skinned = types.Where(
                type =>
                    type == TextureType.RobeLong ||
                    type == TextureType.RobeShort ||
                    type == TextureType.Pants).ToList();

            if (skinned.Count == 0) return;

            if (ignoreTypes != null)
            {
                foreach (var ignore in ignoreTypes)
                {
                    skinned.Remove(ignore);
                    _currentCharacterTextures[ignore].Reset();
                }
            }

            if (skinned.Count == 0) return;

            var skinnedTexture = _currentCharacterTextures[skinned[0]];
            if (skinnedTexture.SelectedTexture == 0)
                skinnedTexture.MoveNext();
        }

        private void ResetTexture(TextureType[] types)
        {
            if (types == null) return;

            foreach (var type in types)
            {
                if (!_currentCharacterTextures.ContainsKey(type))
                    continue;

                _currentCharacterTextures[type].Reset();
            }
        }

        public void OnPrevPortrait() => 
            portrait.sprite = _currentCharacterPortrait.Prev;

        public void OnNextPortrait() => 
            portrait.sprite = _currentCharacterPortrait.Next;

        public int GetSelectedTextureColor(TextureType type) => 
            _currentCharacterTextures[type].SelectedColor;
      
    }
}