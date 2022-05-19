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
    public class MeshManager : MonoBehaviour
    {
        [Serializable]
        public class DefaultMeshValue
        {
            public MeshType type;
            public int value;
        }


        [SerializeField] private RawImage armorRawImage;
        [SerializeField] private RawImage skinMeshRawImage;

        [EnumFlag] public MeshType canChangeMask;
        [HideInInspector] public MeshType[] CanChangeTypes;

        [SerializeField] private DefaultMeshValue[] defaultValues;
        private Dictionary<MeshType, int> _defaultMeshValues;

        [SerializeField] private Material skinMeshRenderMaterial;
        private RenderTexture skinMeshRenderTexture;
        private Material _tmpSkinMeshRenderMaterial;

        [SerializeField] private Material armorMeshRenderMaterial;
        private RenderTexture armorMeshRenderTexture;
        private Material _tmpArmorMeshRenderMaterial;

        public Texture2D ArmorTexture { get; private set; }
        public Texture2D FaceTexture { get; private set; }
        public List<CharacterMeshWrapper> SelectedArmorMeshes { get; private set; }
        public List<CharacterMeshWrapper> SelectedSkinMeshes { get; private set; }
        public bool IsReady { get; private set; }

        private Dictionary<string, Dictionary<MeshType, CharacterMeshWrapper>> _characterMeshes;
        private Dictionary<MeshType, CharacterMeshWrapper> _currentCharacterMeshes;

        private string _characterRace;
        private bool _isLock;
        
        private IMeshLoader _meshLoader;
        private IStaticDataService _staticDataService;
        private IDataManager _dataManager;
        private IMeshInstanceCreator _meshInstanceCreator;
        private IMergeTextureService _mergeTextureService;

        public static MeshManager Instance { get; private set; }

        public Action OnMeshesChanged;
        public Action OnMeshesTextureUpdated;
        private IConfigManager _configManager;

        private void Awake()
        {
            if (Instance != null) Destroy(gameObject);
            Instance = this;

            _characterMeshes = new Dictionary<string, Dictionary<MeshType, CharacterMeshWrapper>>();
            SelectedArmorMeshes = new List<CharacterMeshWrapper>();
            SelectedSkinMeshes = new List<CharacterMeshWrapper>();

            CanChangeTypes = canChangeMask.FlagToArray<MeshType>();

            ArmorTexture = null;
            FaceTexture = new Texture2D(Constants.SKIN_MESHES_ATLAS_SIZE, Constants.SKIN_MESHES_ATLAS_SIZE, TextureFormat.RGB24, false);
            ArmorTexture = new Texture2D(Constants.ARMOR_MESHES_ATLAS_SIZE, Constants.ARMOR_MESHES_ATLAS_SIZE, TextureFormat.RGB24, false);

            _defaultMeshValues = defaultValues.ToDictionary(x => x.type, x => x.value);
            _tmpSkinMeshRenderMaterial = new Material(skinMeshRenderMaterial);
            _tmpArmorMeshRenderMaterial = new Material(armorMeshRenderMaterial);

            _tmpSkinMeshRenderMaterial = skinMeshRenderMaterial;
            _tmpArmorMeshRenderMaterial = armorMeshRenderMaterial;

            armorMeshRenderTexture = new RenderTexture(ArmorTexture.width, ArmorTexture.height, 0, RenderTextureFormat.ARGB32);
            skinMeshRenderTexture = new RenderTexture(FaceTexture.width, FaceTexture.height, 0, RenderTextureFormat.ARGB32);

            var loaderService = AllServices.Container.Single<ILoaderService>();
            _meshLoader = loaderService.MeshLoader;
            _dataManager = loaderService.DataManager;
            _staticDataService = AllServices.Container.Single<IStaticDataService>();
            _meshInstanceCreator = AllServices.Container.Single<IMeshInstanceCreator>();
            _mergeTextureService = AllServices.Container.Single<IMergeTextureService>();

            _configManager = AllServices.Container.Single<IConfigManager>();
            _configManager.OnChangeConfig += OnChangeConfigHandler;
        }

        private void OnDestroy()
        {
            if (_configManager != null)
                _configManager.OnChangeConfig -= OnChangeConfigHandler;
        }

        private Task OnChangeConfigHandler(CharacterGameObjectData data)
        {
            return ApplyConfig(data);
        }

        private async Task ApplyConfig(CharacterGameObjectData data)
        {
            IsReady = false;

            _characterRace = data.Config.folderName;
            InitCurrentCharacterMeshes(data, _characterRace);

            await UpdateTextures();
            while (!IsReady) await Task.Yield();
        }

        private void InitCurrentCharacterMeshes(CharacterGameObjectData data, string characterKey)
        {
            if (_characterMeshes.TryGetValue(characterKey, out _currentCharacterMeshes)) return;

            _currentCharacterMeshes = new Dictionary<MeshType, CharacterMeshWrapper>(EnumComparer.MeshType);
            foreach (var availableMesh in data.Config.availableMeshes)
            {
                var meshType = availableMesh.mesh;
                if (Array.IndexOf(CanChangeTypes, meshType) == -1) continue;
                if (!data.meshBones.TryGetValue(meshType, out var bone)) continue;

                var meshWrapper = new CharacterMeshWrapper(_meshInstanceCreator, _meshLoader, bone, _dataManager, meshType, data.Config);
                if (_defaultMeshValues.TryGetValue(meshType, out var defaultMeshValue))
                {
                    meshWrapper.Mesh.SetMesh(defaultMeshValue);
                    meshWrapper.Mesh.SetTextureAndColor(0, 0);
                }

                _currentCharacterMeshes[meshType] = meshWrapper;
            }

            _characterMeshes[characterKey] = _currentCharacterMeshes;
        }

  
        public bool IsDynamicTextureAtlas()
        {
#if UNITY_EDITOR
            return _staticDataService.LoaderType == LoaderType.AssetDatabase && _staticDataService.MeshAtlasType == MeshAtlasType.Dynamic;
#else
            return false;
#endif
        }

        public void LockUpdate(bool isLock)
        {
            _isLock = isLock;
            if (!_isLock) UpdateModelTextures();
        }

        private async Task UpdateTextures()
        {
            IsReady = false;
            foreach (var meshWrapper in _currentCharacterMeshes.Values)
                while (!meshWrapper.Mesh.IsReady) await Task.Yield();

            BuildTextures();
            IsReady = true;
        }

        /*
         * Create mesh atlas from selected meshes
         */
        private void BuildTextures()
        {
            UpdateSelectedMeshes();

            if (IsDynamicTextureAtlas())
            {
                FaceTexture = BuildMeshAtlas(SelectedSkinMeshes);
                ArmorTexture = BuildMeshAtlas(SelectedArmorMeshes);
            }
            else
            {
                BuildTexture(SelectedSkinMeshes, _tmpSkinMeshRenderMaterial, skinMeshRenderTexture, FaceTexture);
                BuildTexture(SelectedArmorMeshes, _tmpArmorMeshRenderMaterial, armorMeshRenderTexture, ArmorTexture);
            }

            if (!_isLock) UpdateModelTextures();

            OnMeshesChanged?.Invoke();
        }

        private void UpdateSelectedMeshes()
        {
            SelectedArmorMeshes.Clear();
            SelectedSkinMeshes.Clear();

            foreach (var meshWrapper in _currentCharacterMeshes.Values)
            {
                if (meshWrapper.IsEmptyMesh) continue;

                if (meshWrapper.Mesh.IsFaceMesh) SelectedSkinMeshes.Add(meshWrapper);
                else SelectedArmorMeshes.Add(meshWrapper);
            }
        }

        private Texture2D BuildMeshAtlas(List<CharacterMeshWrapper> meshes)
        {
            var textures = new List<Texture2D>();
            foreach (var meshWrapper in meshes)
            {
                if (meshWrapper.IsEmptyMesh) continue;
                textures.Add(meshWrapper.Mesh.Texture.Current);
            }
            return _mergeTextureService.BuildTextureAtlas(Constants.MESH_TEXTURE_SIZE, textures);
        }

        private void BuildTexture(List<CharacterMeshWrapper> meshWrappers, Material material, RenderTexture renderTexture, Texture2D resultTexture)
        {
            var mergeTextures = new Dictionary<string, Texture2D>();
            foreach (var meshWrapper in meshWrappers)
            {
                var mesh = meshWrapper.Mesh;
                var textureName = Helper.GetShaderTextureName(mesh.MeshType);
                if (textureName == null) continue;

                mergeTextures[textureName] = mesh.Texture.Current;
            }

            _mergeTextureService.MergeTextures(material, renderTexture, mergeTextures);
            RenderTexture.active = renderTexture;
            resultTexture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        }

        private void UpdateModelTextures()
        {
            FaceTexture?.Apply();
            ArmorTexture?.Apply();

            if (skinMeshRawImage != null)
            {
                skinMeshRawImage.texture = FaceTexture;
                skinMeshRawImage.enabled = SelectedSkinMeshes.Count != 0;
            }
            if (armorRawImage != null)
            {
                armorRawImage.texture = ArmorTexture;
                armorRawImage.enabled = SelectedArmorMeshes.Count != 0;
            }

            foreach (var meshWrapper in _currentCharacterMeshes.Values)
            {
                if (meshWrapper.Mesh.HasChanges())
                    meshWrapper.ClearPrevMesh();

                if (meshWrapper.IsEmptyMesh) continue;
                meshWrapper.CreateMeshInstance();

                if (IsDynamicTextureAtlas()) continue;

                foreach (var meshRenderer in meshWrapper.MeshInstance.GetComponentsInChildren<MeshRenderer>())
                foreach (var material in meshRenderer.materials)
                    material.mainTexture = meshWrapper.Mesh.IsFaceMesh ? FaceTexture : ArmorTexture;
            }
            OnMeshesTextureUpdated?.Invoke();
        }

      

        #region Mesh Actions
        public void OnNextMesh(IEnumerable<MeshType> types)
        {
            if (!IsReady) return; 
            foreach (var type in types)
            {
                if (_currentCharacterMeshes.ContainsKey(type))
                    _currentCharacterMeshes[type].Mesh.MoveNext();
            }
            OnChangeMesh();
        }

        public void OnPrevMesh(IEnumerable<MeshType> types)
        {
            if (!IsReady) return; 
            foreach (var type in types)
            {
                if (_currentCharacterMeshes.ContainsKey(type))
                    _currentCharacterMeshes[type].Mesh.MovePrev();
            }
            OnChangeMesh();
        }

        public void OnClearMesh(IEnumerable<MeshType> types)
        {
            if (!IsReady) return;
            foreach (var type in types)
            {
                if (_currentCharacterMeshes.ContainsKey(type))
                    _currentCharacterMeshes[type].Mesh.Reset();
            }
            OnChangeMesh();
        }

        public void OnRandom(MeshType[] types, MeshType[][] sameTypes, MeshType[] sameColorTypes, int sameColor = 0)
        {
            if (!IsReady) return; 
            //Remove same meshes from random list
            if (sameTypes != null && sameTypes.Length > 0)
            {
                var typesList = new List<MeshType>(types);
                foreach (var typeGroup in sameTypes)
                {
                    for (var i = 1; i < typeGroup.Length; i++)
                        typesList.Remove(typeGroup[i]);
                }
                types = typesList.ToArray();
            }

            // Shuffle available types
            foreach (var type in types)
            {
                if (!_currentCharacterMeshes.ContainsKey(type)) continue;
                var color = -1;
                if (sameColorTypes != null && Array.IndexOf(sameColorTypes, type) != -1) color = sameColor;
                _currentCharacterMeshes[type].Mesh.Shuffle(color);
            }

            // Setup missing same types
            if (sameTypes != null)
            {
                foreach (var typeGroup in sameTypes)
                {
                    if (typeGroup.Length == 0) continue;

                    var firstMeshForGroup = _currentCharacterMeshes[typeGroup[0]].Mesh;
                    for (var i = 1; i < typeGroup.Length; i++)
                    {
                        if (!_currentCharacterMeshes.TryGetValue(typeGroup[i], out var meshWrapper)) continue;

                        meshWrapper.Mesh.SetMesh(firstMeshForGroup.SelectedMesh);
                        meshWrapper.Mesh.SetTextureAndColor(firstMeshForGroup.Texture.SelectedTexture, firstMeshForGroup.Texture.SelectedColor);
                    }
                }
            }
            OnChangeMesh();
            
        }
        #endregion

        #region Color Actions
        public void OnNextColor(IEnumerable<MeshType> types)
        {
            if (!IsReady) return;
            foreach (var type in types)
            {
                if (_currentCharacterMeshes.ContainsKey(type))
                    _currentCharacterMeshes[type].Mesh.MoveNextColor();
            }
            OnChangeMesh();
        }

        public void OnPrevColor(IEnumerable<MeshType> types)
        {
            if (!IsReady) return; 
            foreach (var type in types)
            {
                if (_currentCharacterMeshes.ContainsKey(type))
                    _currentCharacterMeshes[type].Mesh.MovePrevColor();
            }
            OnChangeMesh();
        }

        public void SetMeshColor(IEnumerable<MeshType> types, int color)
        {
            if (!IsReady) return;
            foreach (var type in types)
            {
                if (_currentCharacterMeshes.ContainsKey(type))
                    _currentCharacterMeshes[type].Mesh.SetColor(color);
            }
            OnChangeMesh();
        }

        public void OnClearMeshColor(IEnumerable<MeshType> types)
        {
            if (!IsReady) return;
            foreach (var type in types)
            {
                if (_currentCharacterMeshes.ContainsKey(type))
                    _currentCharacterMeshes[type].Mesh.ResetColor();
            }
            OnChangeMesh();
        }
        #endregion

        private async void OnChangeMesh()
        {
            await UpdateTextures();
        }
    }
}
