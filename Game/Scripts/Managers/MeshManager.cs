using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Assets.Game.Scripts.Loaders;
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

        public Texture2D ArmorTexture { get; private set; }
        public Texture2D FaceTexture { get; private set; }
        public List<CharacterMeshWrapper> SelectedArmorMeshes { get; private set; }
        public List<CharacterMeshWrapper> SelectedSkinMeshes { get; private set; }
        public bool IsReady { get; private set; }

        private Dictionary<string, Dictionary<MeshType, CharacterMeshWrapper>> _characterMeshes;
        private Dictionary<MeshType, CharacterMeshWrapper> _currentCharacterMeshes;

        private string _characterRace;
        private bool _isLock;
        private Color32[] _emptyPixels;


        private IMeshLoader _meshLoader;
        private IStaticDataService _staticDataService;
        private IDataManager _dataManager;
        private IMeshInstanceCreator _meshInstanceCreator;
        private Dictionary<MeshType, int> _defaultMeshValues;

        public static MeshManager Instance { get; private set; }

        public Action OnMeshesChanged;
        public Action OnMeshesLoaded;
        public Action OnMeshesTextureUpdated;

        void Awake()
        {
            if (Instance != null) Destroy(gameObject);
            Instance = this;

            _characterMeshes = new Dictionary<string, Dictionary<MeshType, CharacterMeshWrapper>>();
            SelectedArmorMeshes = new List<CharacterMeshWrapper>();
            SelectedSkinMeshes = new List<CharacterMeshWrapper>();

            CanChangeTypes = canChangeMask.FlagToArray<MeshType>();

            ArmorTexture = null;
            FaceTexture = new Texture2D(Constants.SKIN_MESHES_ATLAS_SIZE, Constants.SKIN_MESHES_ATLAS_SIZE, TextureFormat.RGB24, false);
            _emptyPixels = new Color32[Constants.MESH_TEXTURE_SIZE * Constants.MESH_TEXTURE_SIZE];

            _defaultMeshValues = defaultValues.ToDictionary(x => x.type, x => x.value);
           
            var loaderService = AllServices.Container.Single<ILoaderService>();
            _meshLoader = loaderService.MeshLoader;
            _dataManager = loaderService.DataManager;
            _staticDataService = AllServices.Container.Single<IStaticDataService>();
            _meshInstanceCreator = AllServices.Container.Single<IMeshInstanceCreator>();

            var configManager = AllServices.Container.Single<IConfigManager>();
            configManager.OnChangeConfig += OnChangeConfigHandler;
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

            _currentCharacterMeshes = new Dictionary<MeshType, CharacterMeshWrapper>();
            foreach (var availableMesh in data.Config.availableMeshes)
            {
                var meshType = availableMesh.mesh;
                if (Array.IndexOf(CanChangeTypes, meshType) == -1) continue;
                if (!data.meshBones.TryGetValue(meshType, out var bone)) continue;

                var meshWrapper = new CharacterMeshWrapper(_meshInstanceCreator, _meshLoader, bone, _dataManager, meshType, data.Config.folderName);
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
            OnMeshesLoaded?.Invoke();

            BuildTexture();
            IsReady = true;

        }

        /*
         * Create mesh atlas from selected meshes
         */
        private void BuildTexture()
        {
            UpdateSelectedMeshes();

            CreateMeshAtlas();

            BuildMeshAtlas(ArmorTexture, SelectedArmorMeshes);
            BuildMeshAtlas(FaceTexture, SelectedSkinMeshes);

            if (!_isLock) UpdateModelTextures();

            OnMeshesChanged?.Invoke();
        }

        private void CreateMeshAtlas()
        {
            var armorSize = Constants.ARMOR_MESHES_ATLAS_SIZE;
            var skinSize = Constants.SKIN_MESHES_ATLAS_SIZE;

            if (IsDynamicTextureAtlas())
            {
                armorSize = SelectedArmorMeshes.Count > 4 ? armorSize :
                    SelectedArmorMeshes.Count > 1 ? armorSize / 2 :
                    armorSize / 4;
                skinSize = SelectedSkinMeshes.Count > 1 ? skinSize : skinSize / 2;
                ArmorTexture = new Texture2D(armorSize, armorSize, TextureFormat.RGB24, false);
                FaceTexture = new Texture2D(skinSize, skinSize, TextureFormat.RGB24, false);
            }

            if (SelectedArmorMeshes.Count > 0 && ArmorTexture == null)
            {
                ArmorTexture = new Texture2D(armorSize, armorSize, TextureFormat.RGB24, false);
            }
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

        private void BuildMeshAtlas(Texture2D atlas, List<CharacterMeshWrapper> meshes)
        {
            if (atlas == null) return;

            var j = 0;
            var meshTextureSize = Constants.MESH_TEXTURE_SIZE;
            var armorSize = atlas.width;
            foreach (var meshWrapper in meshes)
            {
                var selectedMesh = meshWrapper.Mesh;
                var position = IsDynamicTextureAtlas() ? j++ : selectedMesh.MergeOrder;
                var x = meshTextureSize * (position % (armorSize / meshTextureSize));
                var y = (armorSize - meshTextureSize) - (meshTextureSize * position / armorSize) * meshTextureSize;

                var emptyMesh = meshWrapper.IsEmptyMesh;
                if (IsDynamicTextureAtlas() && emptyMesh) continue;

                atlas.SetPixels32(x, y, meshTextureSize, meshTextureSize, 
                    emptyMesh ? _emptyPixels : selectedMesh.Texture.GetPixels32());
            }
        }
        
        private void UpdateModelTextures()
        {
            FaceTexture.Apply();
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
                if (IsDynamicTextureAtlas()) continue;

                foreach (var meshRenderer in meshWrapper.GetOrCreateMeshInstance().GetComponentsInChildren<MeshRenderer>())
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
