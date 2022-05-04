using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace CharacterEditor
{
    public class PrefabShaderManager : MonoBehaviour
    {
        public static PrefabShaderManager Instance { get; set; }

        [SerializeField] private MaterialInfo[] _materials = new MaterialInfo[0];
        public MaterialInfo[] Materials => _materials;

        public Dictionary<string, TextureShaderType> CharacterShaders { get; private set; }
        public TextureShaderType CurrentCharacterShader { get; private set; }

        private string _characterRace;
        private List<SkinnedMeshRenderer> _modelRenderers;
        private List<SkinnedMeshRenderer> _cloakRenderers;


        void Awake()
        {
            if (Instance != null) Destroy(gameObject);
            Instance = this;

            _modelRenderers = new List<SkinnedMeshRenderer>();
            _cloakRenderers = new List<SkinnedMeshRenderer>();

            CharacterShaders = new Dictionary<string, TextureShaderType>();

            MeshManager.Instance.OnMeshesTextureUpdated += OnMeshesTextureUpdatedHandler;

            var configManager = AllServices.Container.Single<IConfigManager>();
            configManager.OnChangeConfig += OnChangeConfigHandler;
        }

        private Task OnChangeConfigHandler(CharacterGameObjectData data)
        {
            return ApplyConfig(data);
        }

        private void OnMeshesTextureUpdatedHandler()
        {
            var materialInfo = GetShaderMaterial(CurrentCharacterShader);
            if (materialInfo == null)
                return;
            
            UpdateMeshShaders(materialInfo);
        }

        private async Task ApplyConfig(CharacterGameObjectData data)
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

            var material = materialInfo.skinMaterial;
            foreach (var render in _modelRenderers)
            {
                material.mainTexture = render.material.mainTexture;
                render.material = material;
            }
            
            var cloakMaterial = materialInfo.cloakMaterial;
            foreach (var render in _cloakRenderers)
            {
                cloakMaterial.mainTexture = render.material.mainTexture;
                render.material = cloakMaterial;
            }
            
            UpdateMeshShaders(materialInfo);
        }

        private void UpdateMeshShaders(MaterialInfo materialInfo)
        {
            var armorMaterial = materialInfo.armorMeshMaterial;
            var faceMaterial = materialInfo.faceMeshMaterial;
            UpdateMeshShaders(MeshManager.Instance.SelectedArmorMeshes, armorMaterial);
            UpdateMeshShaders(MeshManager.Instance.SelectedSkinMeshes, faceMaterial);
        }

        private void UpdateMeshShaders(IEnumerable<CharacterMeshWrapper> meshes, Material material)
        {
            foreach (var meshWrapper in meshes)
            {
                if (meshWrapper.IsEmptyMesh) continue;

                foreach (var render in meshWrapper.GetOrCreateMeshInstance().GetComponentsInChildren<MeshRenderer>())
                {
                    var renderMaterials = new List<Material>();
                    render.GetMaterials(renderMaterials);
                    foreach (var renderMaterial in renderMaterials)
                    {
                        var texture = renderMaterial.mainTexture;
                        renderMaterial.shader = material.shader;
                        renderMaterial.CopyPropertiesFromMaterial(material);
                        renderMaterial.mainTexture = texture;
                    }
                    render.materials = renderMaterials.ToArray();
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

        public void UpdateCloakMaterial(Material mat)
        {
            foreach (var render in _cloakRenderers)
                render.material = mat;
        }
    }
}