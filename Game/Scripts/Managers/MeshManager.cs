using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Assets.Game.Scripts.Loaders;
using CharacterEditor.Mesh;
using CharacterEditor.Services;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UI;

namespace CharacterEditor
{
    public class MeshManager : MonoBehaviour
    {
        [Serializable]
        public class DefaultValues
        {
            public MeshType type;
            public int value;
        }


        [SerializeField] private RawImage armorRawImage;
        [SerializeField] private RawImage skinMeshRawImage;
        [EnumFlag] public MeshType canChangeMask;
        [HideInInspector] public MeshType[] CanChangeTypes;
        [SerializeField] private DefaultValues[] defaultValues;

        public Texture2D ArmorTexture { get; private set; }
        public Texture2D FaceTexture { get; private set; }
        public List<CharacterMesh> SelectedArmorMeshes { get; private set; }
        public List<CharacterMesh> SelectedSkinMeshes { get; private set; }
        public bool IsReady { get; private set; }

        private Dictionary<string, Dictionary<MeshType, CharacterMesh>> _characterMeshes;
        private Dictionary<MeshType, CharacterMesh> _currentCharacterMeshes;

        private string _characterRace;
        private bool _isLock;
        private Coroutine _buildCoroutine;
        private Color32[] _emptyPixels;

        public Action OnMeshesChanged;
        public Action OnMeshesLoaded;
        private IMeshLoader _meshLoader;
        private IStaticDataService _staticDataService;
        private IDataManager _dataManager;

        public static MeshManager Instance { get; private set; }

        void Awake()
        {
            if (Instance != null) Destroy(gameObject);
            Instance = this;

            _characterMeshes = new Dictionary<string, Dictionary<MeshType, CharacterMesh>>();
            SelectedArmorMeshes = new List<CharacterMesh>();
            SelectedSkinMeshes = new List<CharacterMesh>();

            var list = new List<MeshType>();
            foreach (var enumValue in Enum.GetValues(typeof(MeshType)))
            {
                var checkBit = (int)canChangeMask & (int)enumValue;
                if (checkBit != 0) list.Add((MeshType)enumValue);
            }
            CanChangeTypes = list.ToArray();

            ArmorTexture = null;
            FaceTexture = new Texture2D(Constants.SKIN_MESHES_ATLAS_SIZE, Constants.SKIN_MESHES_ATLAS_SIZE, TextureFormat.RGB24, false);
            _emptyPixels = new Color32[Constants.MESH_TEXTURE_SIZE * Constants.MESH_TEXTURE_SIZE];
            for (var i = 0; i < _emptyPixels.Length; i++)
                _emptyPixels[i] = new Color32(0, 0, 0, 0);

            // @todo
            var loaderService = AllServices.Container.Single<ILoaderService>();
            _meshLoader = loaderService.MeshLoader;
            _dataManager = loaderService.DataManager;
            _staticDataService = AllServices.Container.Single<IStaticDataService>();
        }

      

        /*
         * Change Character. Update armor/weapon meshes and fx meshes
         */
        public async Task ApplyConfig(CharacterConfig config, CharacterGameObjectData data)
        {
            IsReady = false;

            _characterRace = config.folderName;
            InitCurentCharacterMehes(config, data, _characterRace);

            foreach (var mesh in _currentCharacterMeshes.Values)
                while (!mesh.IsReady) await Task.Yield();
            
            StartCoroutine(UpdateTextures());
            while (!IsReady) await Task.Yield();

            TextureShaderType shader;
            if (TextureManager.Instance.CharacterShaders.TryGetValue(_characterRace, out shader) && TextureManager.Instance.CurrentCharacterShader != shader)
                SetShader(TextureManager.Instance.CurrentCharacterShader);

            IsReady = true;
        }

        private void InitCurentCharacterMehes(CharacterConfig config, CharacterGameObjectData data, string characterKey)
        {
            if (!_characterMeshes.ContainsKey(characterKey))
            {
                _currentCharacterMeshes = new Dictionary<MeshType, CharacterMesh>();
                foreach (var meshBone in config.availableMeshes)
                {
                    if (Array.IndexOf(CanChangeTypes, meshBone.mesh) == -1) continue;

                    Transform bone;
                    if (!data.meshBones.TryGetValue(meshBone.mesh, out bone)) continue;

                    _currentCharacterMeshes[meshBone.mesh] = MeshFactory.Create(_meshLoader, _dataManager, meshBone.mesh, bone, config.folderName);
                }
                _characterMeshes[characterKey] = _currentCharacterMeshes;

                foreach (var defValue in defaultValues)
                {
                    _currentCharacterMeshes[defValue.type].SetMesh(defValue.value);
                    _currentCharacterMeshes[defValue.type].SetTextureAndColor(0,0);
                }
            }
            _currentCharacterMeshes = _characterMeshes[characterKey];
        }

        /*
         * Update mesh materials and shaders
         */
        public void SetShader(TextureShaderType shader)
        {
            var materialInfo = TextureManager.Instance.GetShaderMaterial(shader);
            if (materialInfo == null)
                return;

            var armorMaterial = materialInfo.armorMeshMaterial;
            var faceMaterial = materialInfo.faceMeshMaterial;
            foreach (var mesh in _currentCharacterMeshes.Values)
            {
                if (mesh.CurrentMesh == null) continue;
                foreach (var render in mesh.CurrentMesh.GetComponentsInChildren<MeshRenderer>())
                {
                    var materials = new List<Material>();
                    render.GetMaterials(materials);
                    foreach (var material in materials)
                    {
                        material.shader = mesh.IsFaceMesh ? faceMaterial.shader : armorMaterial.shader;
                        material.CopyPropertiesFromMaterial(mesh.IsFaceMesh ? faceMaterial : armorMaterial);
                        if (IsDynamicTextureAtlas())
                            material.mainTexture = mesh.Texture.Current;
                        else
                            material.mainTexture = mesh.IsFaceMesh ? FaceTexture : ArmorTexture;
                    }
                    render.materials = materials.ToArray();
                }
            }
        }

        private bool IsDynamicTextureAtlas()
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

        private IEnumerator UpdateTextures()
        {
            IsReady = false;
            foreach (var mesh in _currentCharacterMeshes.Values)
                while (!mesh.IsReady) yield return null;

            if (OnMeshesLoaded != null) OnMeshesLoaded();

            yield return BuildTexture();
        }

        /*
         * Create mesh atlas from selected meshes
         */
        private IEnumerator BuildTexture()
        {
            IsReady = false;

            SelectedArmorMeshes.Clear();
            SelectedSkinMeshes.Clear();
            foreach (var mesh in _currentCharacterMeshes.Values)
            {
                if (mesh.CurrentMesh == null) continue;

                if (mesh.IsFaceMesh) SelectedSkinMeshes.Add(mesh);
                else SelectedArmorMeshes.Add(mesh);
            }

            yield return null;

            //Create empty atlas
            var armorSize = Constants.ARMOR_MESHES_ATLAS_SIZE;
            var skinSize = Constants.SKIN_MESHES_ATLAS_SIZE;

            if (IsDynamicTextureAtlas())
            {
                armorSize = SelectedArmorMeshes.Count > 4 ? armorSize :
                    SelectedArmorMeshes.Count > 1 ? armorSize / 2 :
                    armorSize / 4;
                skinSize = SelectedSkinMeshes.Count > 1 ? skinSize : skinSize / 2;
                ArmorTexture = new Texture2D(armorSize, armorSize, TextureFormat.RGB24, false);
                yield return null;
                FaceTexture = new Texture2D(skinSize, skinSize, TextureFormat.RGB24, false);
                yield return null;
            }

            if (SelectedArmorMeshes.Count > 0 && ArmorTexture == null)
            {
                ArmorTexture = new Texture2D(armorSize, armorSize, TextureFormat.RGB24, false);
                yield return null;
            }

            if (ArmorTexture != null)
            {
                CreateMeshAtlas(ArmorTexture, SelectedArmorMeshes);
                yield return null;
            }

            CreateMeshAtlas(FaceTexture, SelectedSkinMeshes);
            yield return null;

            if (!_isLock)
            {
                UpdateModelTextures();
                yield return null;
            }
            IsReady = true;
            OnMeshesChanged?.Invoke();
        }

        private void CreateMeshAtlas(Texture2D atlas, List<CharacterMesh> meshes)
        {
            Profiler.BeginSample("CreateMeshAtlas");
            var j = 0;
            const int meshTextureSize = Constants.MESH_TEXTURE_SIZE;
            var armorSize = atlas.width;
            foreach (var selectedMesh in meshes)
            {
                var position = IsDynamicTextureAtlas() ? j++ : selectedMesh.MergeOrder;
                var x = meshTextureSize * (position % (armorSize / meshTextureSize));
                var y = (armorSize - meshTextureSize) - (meshTextureSize * position / armorSize) * meshTextureSize;

                var emptyMesh = selectedMesh.CurrentMesh == null;
                if (IsDynamicTextureAtlas() && emptyMesh) continue;

                atlas.SetPixels32(x, y, meshTextureSize, meshTextureSize, 
                    emptyMesh ? _emptyPixels : selectedMesh.Texture.GetPixels32());
            }
            Profiler.EndSample();
        }
        
        private void UpdateModelTextures()
        {
            if (FaceTexture != null) FaceTexture.Apply();
            if (ArmorTexture != null) ArmorTexture.Apply();
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

            foreach (var mesh in _currentCharacterMeshes.Values)
            {
                mesh.UpdateMesh();
                if (mesh.CurrentMesh == null) continue;

                mesh.CurrentMesh.SetActive(true);
                if (IsDynamicTextureAtlas()) continue;

                foreach (var meshRenderer in mesh.CurrentMesh.GetComponentsInChildren<MeshRenderer>())
                foreach (var material in meshRenderer.materials)
                    material.mainTexture = mesh.IsFaceMesh ? FaceTexture : ArmorTexture;
            }

          //  if (!IsDynamicTextureAtlas())
                SetShader(TextureManager.Instance.CurrentCharacterShader);
        }

        #region Mesh Actions
        public void OnNextMesh(IEnumerable<MeshType> types)
        {
            if (!IsReady) return; 
            foreach (var type in types)
            {
                if (_currentCharacterMeshes.ContainsKey(type))
                    _currentCharacterMeshes[type].MoveNext();
            }
            OnChangeMesh();
        }

        public void OnPrevMesh(IEnumerable<MeshType> types)
        {
            if (!IsReady) return; 
            foreach (var type in types)
            {
                if (_currentCharacterMeshes.ContainsKey(type))
                    _currentCharacterMeshes[type].MovePrev();
            }
            OnChangeMesh();
        }

        public void OnClearMesh(IEnumerable<MeshType> types)
        {
            if (!IsReady) return;
            foreach (var type in types)
            {
                if (_currentCharacterMeshes.ContainsKey(type))
                    _currentCharacterMeshes[type].Reset();
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
                _currentCharacterMeshes[type].Shuffle(color);
            }

            // Setup missing same types
            if (sameTypes != null)
            {
                foreach (var typeGroup in sameTypes)
                {
                    for (var i = 1; i < typeGroup.Length; i++)
                    {
                        if (!_currentCharacterMeshes.ContainsKey(typeGroup[i])) continue;

                        var mesh = _currentCharacterMeshes[typeGroup[0]];
                        _currentCharacterMeshes[typeGroup[i]].SetMesh(mesh.SelectedMesh);
                        _currentCharacterMeshes[typeGroup[i]].SetTextureAndColor(mesh.Texture.SelectedTexture, mesh.Texture.SelectedColor);
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
                    _currentCharacterMeshes[type].MoveNextColor();
            }
            OnChangeMesh();
        }

        public void OnPrevColor(IEnumerable<MeshType> types)
        {
            if (!IsReady) return; 
            foreach (var type in types)
            {
                if (_currentCharacterMeshes.ContainsKey(type))
                    _currentCharacterMeshes[type].MovePrevColor();
            }
            OnChangeMesh();
        }

        public void SetMeshColor(IEnumerable<MeshType> types, int color)
        {
            if (!IsReady) return;
            foreach (var type in types)
            {
                if (_currentCharacterMeshes.ContainsKey(type))
                    _currentCharacterMeshes[type].SetColor(color);
            }
            OnChangeMesh();
        }

        public void OnClearMeshColor(IEnumerable<MeshType> types)
        {
            if (!IsReady) return;
            foreach (var type in types)
            {
                if (_currentCharacterMeshes.ContainsKey(type))
                    _currentCharacterMeshes[type].ResetColor();
            }
            OnChangeMesh();
        }
        #endregion

        private void OnChangeMesh()
        {
            if (_buildCoroutine != null) StopCoroutine(_buildCoroutine);
            _buildCoroutine = StartCoroutine(UpdateTextures());
        }
    }
}
