using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace CharacterEditor
{

    public class PrefabShaderManager : MonoBehaviour
    {
        [Serializable]
        public class MaterialInfo
        {
            public TextureShaderType shader;

            [SerializeField]
            private Material material;

            private Dictionary<MaterialType, Material> _materialInstance = new Dictionary<MaterialType, Material>(5, EnumComparer.MaterialType);
            public Material GetMaterial(MaterialType type)
            {
                if (!_materialInstance.TryGetValue(type, out var materialInstance))
                {
                    materialInstance = new Material(material);
                    _materialInstance[type] = materialInstance;
                }

                return materialInstance;
            }
        }

        public static PrefabShaderManager Instance { get; private set; }

        [SerializeField] private MaterialInfo[] _materials = new MaterialInfo[0];
        public MaterialInfo[] Materials => _materials;

        private Dictionary<string, TextureShaderType> _characterShaders;
        private TextureShaderType _currentShaderType;

        private List<Renderer> _modelRenderers;
        private List<Renderer> _cloakRenderers;

        private MeshManager _meshManager;
        private IConfigManager _configManager;

        public MaterialInfo GetShaderMaterial() =>
            GetShaderMaterial(_currentShaderType);

        private void Awake()
        {
            if (Instance != null) Destroy(gameObject);
            Instance = this;

            _modelRenderers = new List<Renderer>();
            _cloakRenderers = new List<Renderer>();

            _characterShaders = new Dictionary<string, TextureShaderType>();

            _configManager = AllServices.Container.Single<IConfigManager>();
            _configManager.OnChangeConfig += OnChangeConfigHandler;

            _meshManager = GetComponent<MeshManager>();
            _meshManager.OnMeshesUpdated += OnMeshesTextureUpdatedHandler;
        }

        private void OnDestroy()
        {
            if (_configManager != null)
                _configManager.OnChangeConfig -= OnChangeConfigHandler;

            if (_meshManager != null)
                _meshManager.OnMeshesUpdated -= OnMeshesTextureUpdatedHandler;
        }

        public void UpdateCharacterMaterials(TextureShaderType shader)
        {
            var materialInfo = GetShaderMaterial(shader);
            if (materialInfo == null)
                return;

            UpdateSkinsMaterial(materialInfo);
            UpdateMeshesMaterial(materialInfo);
        }

        private Task OnChangeConfigHandler(CharacterGameObjectData data)
        {
            ApplyConfig(data);
            return Task.CompletedTask;
        }

        private void OnMeshesTextureUpdatedHandler()
        {
            var materialInfo = GetShaderMaterial(_currentShaderType);
            if (materialInfo == null)
                return;

            UpdateMeshesMaterial(materialInfo);
        }

        private void ApplyConfig(CharacterGameObjectData data)
        {
            _modelRenderers.Clear();
            _modelRenderers.AddRange(data.SkinMeshes);
            _modelRenderers.AddRange(data.ShortRobeMeshes);
            _modelRenderers.AddRange(data.LongRobeMeshes);

            _cloakRenderers.Clear();
            _cloakRenderers.AddRange(data.CloakMeshes);

            var characterRace = data.Config.folderName;
            if (_characterShaders.TryGetValue(characterRace, out var shaderType) && shaderType == _currentShaderType)
                return;

            _characterShaders[characterRace] = _currentShaderType;
            UpdateCharacterMaterials(_currentShaderType);
        }

        private void UpdateSkinsMaterial(MaterialInfo materialInfo)
        {
            var material = materialInfo.GetMaterial(MaterialType.Skin);
            foreach (var render in _modelRenderers)
            {
                material.mainTexture = render.material.mainTexture;
                render.material = material;
            }

            var cloakMaterial = materialInfo.GetMaterial(MaterialType.Cloak);
            foreach (var render in _cloakRenderers)
            {
                cloakMaterial.mainTexture = render.material.mainTexture;
                render.material = cloakMaterial;
            }
        }

        private void UpdateMeshesMaterial(MaterialInfo materialInfo)
        {
            UpdateMeshesMaterial(_meshManager.SelectedArmorMeshes, materialInfo.GetMaterial(MaterialType.Armor));
            UpdateMeshesMaterial(_meshManager.SelectedSkinMeshes, materialInfo.GetMaterial(MaterialType.Face));
        }

        private void UpdateMeshesMaterial(IEnumerable<CharacterMeshWrapper> meshes, Material material)
        {
            foreach (var meshWrapper in meshes)
            {
                if (meshWrapper.IsEmptyMesh) continue;

                foreach (var render in meshWrapper.MeshInstance.GetComponentsInChildren<MeshRenderer>())
                {
                    var tmpMaterials = render.materials;
                    var replacedMaterials = new Material[tmpMaterials.Length];
                    for (var i = 0; i < tmpMaterials.Length; i++)
                    {
                        var currentMaterial = tmpMaterials[i];
                        replacedMaterials[i] = _meshManager.IsDynamicTextureAtlas ? new Material(material) : material;
                        replacedMaterials[i].mainTexture = currentMaterial.mainTexture;
                    }

                    render.materials = replacedMaterials;
                }
            }
        }

        private MaterialInfo GetShaderMaterial(TextureShaderType shader)
        {
            foreach (var mat in _materials)
                if (mat.shader == shader) return mat;

            return null;
        }
    }
}