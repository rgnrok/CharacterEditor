using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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

        [SerializeField] private Material faceMeshRenderMaterial;
        private RenderTexture _faceMeshRenderTexture;
        private Material _tmpFaceMeshRenderMaterial;

        [SerializeField] private Material armorMeshRenderMaterial;
        private RenderTexture _armorMeshRenderTexture;
        private Material _tmpArmorMeshRenderMaterial;

        public static MeshManager Instance { get; private set; }

        public Texture2D ArmorTexture { get; private set; }
        public Texture2D FaceTexture { get; private set; }
        public List<CharacterMeshWrapper> SelectedArmorMeshes { get; private set; }
        public List<CharacterMeshWrapper> SelectedSkinMeshes { get; private set; }
        public bool IsReady { get; private set; }

        public bool IsDynamicTextureAtlas
        {
            get
            {
#if UNITY_EDITOR
                return _staticDataService.LoaderType == LoaderType.AssetDatabase && _staticDataService.MeshAtlasType == MeshAtlasType.Dynamic;
#else
                return false;
#endif
            }
        }

        private IConfigManager _configManager;
        private ILoaderService _loaderService;
        private IStaticDataService _staticDataService;
        private IMeshInstanceCreator _meshInstanceCreator;
        private IMergeTextureService _mergeTextureService;

        private Dictionary<string, Dictionary<MeshType, CharacterMeshWrapper>> _characterMeshes;
        private Dictionary<MeshType, CharacterMeshWrapper> _currentCharacterMeshes;

        private bool _isLock;
        private CancellationTokenSource _mergeTextureCancellationToken;

        public event Action OnMeshesChanged;
        public event Action OnMeshesUpdated;

        private void Awake()
        {
            if (Instance != null) Destroy(gameObject);
            Instance = this;

            _characterMeshes = new Dictionary<string, Dictionary<MeshType, CharacterMeshWrapper>>();
            SelectedArmorMeshes = new List<CharacterMeshWrapper>();
            SelectedSkinMeshes = new List<CharacterMeshWrapper>();

            CanChangeTypes = canChangeMask.FlagToArray<MeshType>();

            FaceTexture = new Texture2D(Constants.SKIN_MESHES_ATLAS_SIZE, Constants.SKIN_MESHES_ATLAS_SIZE, TextureFormat.RGB24, false);
            ArmorTexture = new Texture2D(Constants.ARMOR_MESHES_ATLAS_SIZE, Constants.ARMOR_MESHES_ATLAS_SIZE, TextureFormat.RGB24, false);

            _defaultMeshValues = defaultValues.ToDictionary(x => x.type, x => x.value);
            _tmpFaceMeshRenderMaterial = new Material(faceMeshRenderMaterial);
            _tmpArmorMeshRenderMaterial = new Material(armorMeshRenderMaterial);

            _armorMeshRenderTexture = new RenderTexture(ArmorTexture.width, ArmorTexture.height, 0, RenderTextureFormat.ARGB32);
            _faceMeshRenderTexture = new RenderTexture(FaceTexture.width, FaceTexture.height, 0, RenderTextureFormat.ARGB32);

            _loaderService = AllServices.Container.Single<ILoaderService>();
            _staticDataService = AllServices.Container.Single<IStaticDataService>();
            _meshInstanceCreator = AllServices.Container.Single<IMeshInstanceCreator>();
            _mergeTextureService = AllServices.Container.Single<IMergeTextureService>();

            _configManager = AllServices.Container.Single<IConfigManager>();
            _configManager.OnChangeConfig += OnChangeConfigHandler;

            _mergeTextureCancellationToken = new CancellationTokenSource();

        }

        private void OnDestroy()
        {
            if (_configManager != null)
                _configManager.OnChangeConfig -= OnChangeConfigHandler;

            if (_mergeTextureCancellationToken != null)
            {
                _mergeTextureCancellationToken.Cancel();
                _mergeTextureCancellationToken.Dispose();
            }
        }

        public void LockUpdate(bool isLock)
        {
            if (_isLock == isLock) return;

            _isLock = isLock;
            ApplyMeshTextures();
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

                        meshWrapper.Mesh.SetMesh(firstMeshForGroup.SelectedMeshIndex);
                        meshWrapper.Mesh.SetTextureAndColor(firstMeshForGroup.Texture.SelectedTextureIndex, firstMeshForGroup.Texture.SelectedColorIndex);
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

        private async Task OnChangeConfigHandler(CharacterGameObjectData data)
        {
            InitCurrentCharacterMeshes(data, data.Config.folderName);
            await UpdateMeshes(_mergeTextureCancellationToken.Token);
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

                var characterMesh = MeshFactory.Create(_loaderService.MeshLoader, _loaderService.TextureLoader, _loaderService.DataManager, meshType, data.Config);
                var meshWrapper = new CharacterMeshWrapper(_meshInstanceCreator, bone, characterMesh);
                if (_defaultMeshValues.TryGetValue(meshType, out var defaultMeshValue))
                {
                    meshWrapper.Mesh.SetMesh(defaultMeshValue);
                    meshWrapper.Mesh.SetTextureAndColor(0, 0);
                }

                _currentCharacterMeshes[meshType] = meshWrapper;
            }

            _characterMeshes[characterKey] = _currentCharacterMeshes;
        }
        
        private async Task UpdateMeshes(CancellationToken token)
        {
            IsReady = false;
            foreach (var meshWrapper in _currentCharacterMeshes.Values)
            {
                while (!meshWrapper.Mesh.IsReady)
                {
                    if (token.IsCancellationRequested)
                    {
                        IsReady = true;
                        return;
                    }
                    await Task.Yield();
                }
            }

            BuildTextures();

            ApplyMeshTextures();

            IsReady = true;
            OnMeshesChanged?.Invoke();
        }

        private void BuildTextures()
        {
            UpdateSelectedMeshes();

            if (IsDynamicTextureAtlas)
            {
                FaceTexture = BuildMeshAtlas(SelectedSkinMeshes);
                ArmorTexture = BuildMeshAtlas(SelectedArmorMeshes);
            }
            else
            {
                BuildTexture(SelectedSkinMeshes, _tmpFaceMeshRenderMaterial, _faceMeshRenderTexture, FaceTexture);
                BuildTexture(SelectedArmorMeshes, _tmpArmorMeshRenderMaterial, _armorMeshRenderTexture, ArmorTexture);
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

        private Texture2D BuildMeshAtlas(IEnumerable<CharacterMeshWrapper> meshes)
        {
            var textures = new List<Texture2D>();
            foreach (var meshWrapper in meshes)
            {
                if (meshWrapper.IsEmptyMesh) continue;
                textures.Add(meshWrapper.Mesh.Texture.Current);
            }
            return _mergeTextureService.BuildTextureAtlas(Constants.MESH_TEXTURE_SIZE, textures);
        }

        private void BuildTexture(IEnumerable<CharacterMeshWrapper> meshWrappers, Material material, RenderTexture renderTexture, Texture2D resultTexture)
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

        private void ApplyMeshTextures()
        {
            if (_isLock) return;

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

                if (IsDynamicTextureAtlas) continue;

                foreach (var meshRenderer in meshWrapper.MeshRenders)
                foreach (var material in meshRenderer.materials)
                    material.mainTexture = meshWrapper.Mesh.IsFaceMesh ? FaceTexture : ArmorTexture;
            }
            OnMeshesUpdated?.Invoke();
        }
        
        private async void OnChangeMesh()
        {
            ResetCancellationTokenSource();
            await UpdateMeshes(_mergeTextureCancellationToken.Token);
        }

        private void ResetCancellationTokenSource()
        {
            _mergeTextureCancellationToken.Cancel();
            _mergeTextureCancellationToken.Dispose();
            _mergeTextureCancellationToken = new CancellationTokenSource();
        }
    }
}
