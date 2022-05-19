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

            private Dictionary<MaterialType, Material> _materialInstance = new Dictionary<MaterialType, Material>(4, EnumComparer.MaterialType);
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

        public Dictionary<string, TextureShaderType> CharacterShaders { get; private set; }
        public TextureShaderType CurrentCharacterShader { get; private set; }

        private List<Renderer> _modelRenderers;
        private List<Renderer> _cloakRenderers;

        private string _characterRace;
        private MeshManager _meshManager;
        private IConfigManager _configManager;


        private void Awake()
        {
            if (Instance != null) Destroy(gameObject);
            Instance = this;

            _modelRenderers = new List<Renderer>();
            _cloakRenderers = new List<Renderer>();

            CharacterShaders = new Dictionary<string, TextureShaderType>();

            _meshManager = MeshManager.Instance;
            _meshManager.OnMeshesTextureUpdated += OnMeshesTextureUpdatedHandler;

            _configManager = AllServices.Container.Single<IConfigManager>();
            _configManager.OnChangeConfig += OnChangeConfigHandler;
        }

        private void OnDestroy()
        {
            if (_configManager != null)
                _configManager.OnChangeConfig -= OnChangeConfigHandler;

            if (_meshManager != null)
                _meshManager.OnMeshesTextureUpdated -= OnMeshesTextureUpdatedHandler;
        }

        private Task OnChangeConfigHandler(CharacterGameObjectData data)
        {
            ApplyConfig(data);
            return Task.CompletedTask;
        }

        private void OnMeshesTextureUpdatedHandler()
        {
            var materialInfo = GetShaderMaterial(CurrentCharacterShader);
            if (materialInfo == null)
                return;
            
            UpdateMeshShaders(materialInfo);
        }

        private void ApplyConfig(CharacterGameObjectData data)
        {
            _characterRace = data.Config.folderName;

            _modelRenderers.Clear();
            _modelRenderers.AddRange(data.SkinMeshes);
            _modelRenderers.AddRange(data.ShortRobeMeshes);
            _modelRenderers.AddRange(data.LongRobeMeshes);

            _cloakRenderers.Clear();
            _cloakRenderers.AddRange(data.CloakMeshes);

            if (!CharacterShaders.ContainsKey(_characterRace) || CharacterShaders[_characterRace] != CurrentCharacterShader)
                SetShader(CurrentCharacterShader);
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
            
            UpdateMeshShaders(materialInfo);
        }

        private void UpdateMeshShaders(MaterialInfo materialInfo)
        {
            UpdateMeshShaders(MeshManager.Instance.SelectedArmorMeshes, materialInfo.GetMaterial(MaterialType.Armor));
            UpdateMeshShaders(MeshManager.Instance.SelectedSkinMeshes, materialInfo.GetMaterial(MaterialType.Face));
        }

        private void UpdateMeshShaders(IEnumerable<CharacterMeshWrapper> meshes, Material material)
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
                        replacedMaterials[i] = _meshManager.IsDynamicTextureAtlas() ? new Material(material) : material;
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

        public MaterialInfo GetShaderMaterial()
        {
            return GetShaderMaterial(CurrentCharacterShader);
        }
    }
}