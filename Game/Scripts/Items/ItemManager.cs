using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using CharacterEditor.Services;
using UnityEngine;
using UnityEngine.UI;

namespace CharacterEditor
{
    namespace CharacterInventory
    {
        public class ItemManager : MonoBehaviour
        {
            private struct MeshInstanceInfo
            {
                public MeshType meshType;
                public List<GameObject> gameObjects;

                public MeshInstanceInfo(MeshType type, List<GameObject> objects)
                {
                    meshType = type;
                    gameObjects = objects;
                }
            }

            [SerializeField] private RawImage skinRawImage;
            [SerializeField] private RawImage armorRawImage;


            [SerializeField] private Material clothRenderShaderMaterial;
            [SerializeField] private Material armorRenderShaderMaterial;


            private Dictionary<string, Texture2D> _characterTextures;
            public Texture2D CharacterTexture
            {
                get
                {
                    if (!_characterTextures.ContainsKey(_currentCharacter.Guid))
                        _characterTextures[_currentCharacter.Guid] = new Texture2D(Constants.SKIN_TEXTURE_ATLAS_SIZE, Constants.SKIN_TEXTURE_ATLAS_SIZE, TextureFormat.RGB24, false);

                    return _characterTextures[_currentCharacter.Guid];
                }
            }

            private Dictionary<string, Texture2D> _armorTextures;
            public Texture2D ArmorTexture
            {
                get
                {
                    if (!_armorTextures.ContainsKey(_currentCharacter.Guid))
                        _armorTextures[_currentCharacter.Guid] = new Texture2D(Constants.ARMOR_MESHES_ATLAS_SIZE, Constants.ARMOR_MESHES_ATLAS_SIZE, TextureFormat.RGB24, false);

                    return _armorTextures[_currentCharacter.Guid];
                }
            }

            private Dictionary<string, Texture2D> _cloakTextures;
            public Texture2D CloakTexture
            {
                get
                {
                    if (!_cloakTextures.ContainsKey(_currentCharacter.Guid))
                        _cloakTextures[_currentCharacter.Guid] = null;

                    return _cloakTextures[_currentCharacter.Guid];
                }
            }

            private Character _currentCharacter;
            private readonly List<Renderer> _modelRenders = new List<Renderer>();
            private readonly List<Renderer> _shortRobeRenders = new List<Renderer>();
            private readonly List<Renderer> _longRobeRenders = new List<Renderer>();
            private readonly List<Renderer> _cloakRenders = new List<Renderer>();

            private TextureShaderType _currentShaderType;
            private Shader _particleShader;

         
            private Dictionary<string, Dictionary<string, MeshInstanceInfo>> _meshInstances;
            // Queue for destroy item gameobjects and unload bundle info
            private readonly List<EquipItem> _unequipItemsQueue = new List<EquipItem>();
            // Queue for destroy only item gameobjects, without unload bundle info
            private readonly List<GameObject> _destroyMeshQueue = new List<GameObject>();

            public Action<EquipItem> OnEquip;
            public Action<EquipItem> OnUnEquip;

            public static ItemManager Instance { get; private set; }

            public Material defaultMaterial;
            private Material faceMeshMaterial;
            private Dictionary<string, Dictionary<MaterialType, Material>> _characterMaterials;
            private MergeTextureService _mergeTextureService;

            private Material _tmpArmorRenderShaderMaterial;
            private Material _tmpClothRenderShaderMaterial;
            private RenderTexture _renderClothTexture;
            private RenderTexture _renderArmorTexture;
            private ISaveLoadService _saveLoadService;


            private void Awake()
            {
                if (Instance != null) Destroy(gameObject);
                Instance = this;

                _armorTextures = new Dictionary<string, Texture2D>();
                _characterTextures = new Dictionary<string, Texture2D>();
                _cloakTextures = new Dictionary<string, Texture2D>();
                _characterMaterials = new Dictionary<string, Dictionary<MaterialType, Material>>();

                _meshInstances = new Dictionary<string, Dictionary<string, MeshInstanceInfo>>();
                _particleShader = Shader.Find("Particles/Additive (Soft)");

                _mergeTextureService = new MergeTextureService();

                _tmpArmorRenderShaderMaterial = new Material(armorRenderShaderMaterial);
                _tmpClothRenderShaderMaterial = new Material(clothRenderShaderMaterial);

                _renderArmorTexture = new RenderTexture(Constants.ARMOR_MESHES_ATLAS_SIZE, Constants.ARMOR_MESHES_ATLAS_SIZE, 0, RenderTextureFormat.ARGB32);
                _renderClothTexture = new RenderTexture(Constants.SKIN_TEXTURE_ATLAS_SIZE, Constants.SKIN_TEXTURE_ATLAS_SIZE, 0, RenderTextureFormat.ARGB32);


                // _saveLoadService = AllServices.Container.Single<ISaveLoadService>();
                // _saveLoadService.OnCharactersLoaded += OnCharactersLoadedHandler;
            }


            /*
            * Change Character. Update textures and skin meshes
            */
            public void SetCharacter(Character character)
            {
                if (_currentCharacter != null)
                {
                    if (_currentCharacter.Guid == character.Guid) return;
                    _currentCharacter.OnUnEquipItem -= UnEquipItemHandler;
                }

                _currentCharacter = character;
                _currentCharacter.OnUnEquipItem += UnEquipItemHandler;

                _modelRenders.Clear();
                _modelRenders.AddRange(character.GameObjectData.SkinMeshes);
                if (character.GameObjectData.PreviewSkinMeshes != null)
                    _modelRenders.AddRange(character.GameObjectData.PreviewSkinMeshes);

                _shortRobeRenders.Clear();
                _shortRobeRenders.AddRange(character.GameObjectData.ShortRobeMeshes);
                if (character.GameObjectData.PreviewShortRobeMeshes != null)
                    _shortRobeRenders.AddRange(character.GameObjectData.PreviewShortRobeMeshes);

                _longRobeRenders.Clear();
                _longRobeRenders.AddRange(character.GameObjectData.LongRobeMeshes);
                if (character.GameObjectData.PreviewLongRobeMeshes != null)
                    _longRobeRenders.AddRange(character.GameObjectData.PreviewLongRobeMeshes);

                _cloakRenders.Clear();
                _cloakRenders.AddRange(character.GameObjectData.CloakMeshes);
                if (character.GameObjectData.PreviewCloakMeshes != null)
                    _cloakRenders.AddRange(character.GameObjectData.PreviewCloakMeshes);


                Dictionary<MaterialType, Material> chMaterials;
                if (!_characterMaterials.TryGetValue(_currentCharacter.Guid, out chMaterials))
                {
                    chMaterials = new Dictionary<MaterialType, Material>
                    {
                        {MaterialType.Skin, new Material(defaultMaterial)},
                        {MaterialType.Face, new Material(defaultMaterial)},
                        {MaterialType.Cloak, new Material(defaultMaterial)},
                        {MaterialType.Armor, new Material(defaultMaterial)},
                    };
                    _characterMaterials[_currentCharacter.Guid] = chMaterials;
                }

                // always update material for renderers
                foreach (var ren in _modelRenders) ren.material = chMaterials[MaterialType.Skin];
                foreach (var ren in _shortRobeRenders) ren.material = chMaterials[MaterialType.Skin];
                foreach (var ren in _longRobeRenders) ren.material = chMaterials[MaterialType.Skin];
                foreach (var ren in _cloakRenders) ren.material = chMaterials[MaterialType.Cloak];

                faceMeshMaterial = chMaterials[MaterialType.Face];

                skinRawImage.texture = CharacterTexture;
                armorRawImage.texture = ArmorTexture;
            }

            private void UpdateModelTextures()
            {
                foreach (var item in _unequipItemsQueue)
                {
                    var meshes = GetItemMeshes(item.Guid);
                    foreach (var mesh in meshes.gameObjects) Destroy(mesh);
                    meshes.gameObjects.Clear();

                    item.ItemMesh.UnloadTexturesAndMesh(_currentCharacter.ConfigGuid);
                }
                _unequipItemsQueue.Clear();

                foreach (var go in _destroyMeshQueue)
                    Destroy(go);
                _destroyMeshQueue.Clear();

                UpdateTexturesAndMeshes();
            }

            public bool CanEquip(EquipItem item)
            {
                return true; // todo check character parameters, class, and etc
            }

            public void UnEquipItem(EquipItem item)
            {
                if (!_currentCharacter.IsEquip(item)) return;

                _currentCharacter.UnEquipItem(item);
                BuildTextures();
                if (OnUnEquip != null) OnUnEquip(item);
            }

            public async Task EquipItem(EquipItem item, EquipItemSlot slotType = EquipItemSlot.Undefined)
            {
                await EquipItemInner(item, slotType);
                BuildTextures();
            }

            public async Task EquipItems(Dictionary<EquipItemSlot, EquipItem> equipItems)
            {
                foreach (var itemPair in equipItems)
                    await EquipItemInner(itemPair.Value, itemPair.Key);

                BuildTextures();
            }

            public async void SwapEquipedItem(EquipItemSlot slotType1, EquipItemSlot slotType2)
            {
                if (!_currentCharacter.EquipItems.TryGetValue(slotType1, out var item1) || !_currentCharacter.EquipItems.TryGetValue(slotType2, out var item2)) return;
                if (!_currentCharacter.IsEquip(item1) || !_currentCharacter.IsEquip(item2)) return;

                var meshInfo1 = GetItemMeshes(item1.Guid);
                var meshInfo2 = GetItemMeshes(item2.Guid);
                foreach (var go in meshInfo1.gameObjects)
                    _destroyMeshQueue.Add(go);

                foreach (var go in meshInfo2.gameObjects)
                    _destroyMeshQueue.Add(go);

                meshInfo1.gameObjects.Clear();
                meshInfo2.gameObjects.Clear();

                // Left and Right hand has different mesh models
                await item1.ItemMesh.LoadTexturesAndMeshes(_currentCharacter.ConfigGuid, slotType2.IsAdditionalSlot());
                await item2.ItemMesh.LoadTexturesAndMeshes(_currentCharacter.ConfigGuid, slotType1.IsAdditionalSlot());

                _currentCharacter.SwapItems(slotType1, slotType2);

                InstanceMesh(item1, slotType2);
                InstanceMesh(item2, slotType1);
                BuildTextures();
            }

         

            private async Task EquipItemInner(EquipItem equipItem, EquipItemSlot slotType)
            {
                if (_currentCharacter.IsEquip(equipItem))
                {
                    _currentCharacter.UnEquipItem(equipItem);
                    OnUnEquip?.Invoke(equipItem);
                    return;
                }

                slotType = _currentCharacter.EquipItem(equipItem, slotType);
                if (slotType == EquipItemSlot.Undefined) return;

                await equipItem.ItemMesh.LoadTexturesAndMeshes(_currentCharacter.ConfigGuid, slotType.IsAdditionalSlot());

                InstanceMesh(equipItem, slotType);
                OnEquip?.Invoke(equipItem);
            }
            
            private MeshInstanceInfo GetItemMeshes(string itemGuid)
            {
              
                var emptyDic = new MeshInstanceInfo(MeshType.Undefined, new List<GameObject>());
                if (!_meshInstances.ContainsKey(_currentCharacter.Guid) ||
                    !_meshInstances[_currentCharacter.Guid].ContainsKey(itemGuid)) return emptyDic;

                return _meshInstances[_currentCharacter.Guid][itemGuid];
            }

            private void InstanceMesh(string itemGuid, ItemMesh mesh, Transform bone, int layer = 0)
            {
                if (!_meshInstances.ContainsKey(_currentCharacter.Guid))
                    _meshInstances[_currentCharacter.Guid] = new Dictionary<string, MeshInstanceInfo>();

                if (!_meshInstances[_currentCharacter.Guid].ContainsKey(itemGuid))
                    _meshInstances[_currentCharacter.Guid][itemGuid] = new MeshInstanceInfo(mesh.MeshType, new List<GameObject>());

                _meshInstances[_currentCharacter.Guid][itemGuid].gameObjects.Add(mesh.InstanceMesh(bone, layer, false, false));//why disabled lod in old code ?
            }

            private void InstanceMesh(EquipItem item, EquipItemSlot slot)
            {
                var meshes = item.ItemMesh.GetItemMeshes(_currentCharacter.ConfigGuid, slot.IsAdditionalSlot());
                var meshType = Helper.GetHandMeshTypeBySlot(slot);

                foreach (var mesh in meshes)
                {
                    var boneMesh = meshType != MeshType.Undefined ? meshType : mesh.MeshType;

                    if (_currentCharacter.GameObjectData.meshBones.TryGetValue(boneMesh, out var bone))
                        InstanceMesh(item.Guid, mesh, bone);

                    if (_currentCharacter.GameObjectData.previewMeshBones != null)
                    {
                        if (_currentCharacter.GameObjectData.previewMeshBones.TryGetValue(boneMesh, out var previewBone))
                            InstanceMesh(item.Guid, mesh, previewBone, Constants.LAYER_CHARACTER_PREVIEW);
                    }
                }
            }

            private void UnEquipItemHandler(EquipItem item)
            {
                _unequipItemsQueue.Add(item);
            }

            private void BuildTextures()
            {
                MergeClothTextures();
                BuildArmorTexture();

                UpdateModelTextures();
            }
            
            private void MergeClothTextures()
            {
                var mergeTextures = new Dictionary<string, Texture2D>(_currentCharacter.EquipItems.Count)
                {
                    ["_SkinTex"] = _currentCharacter.Texture
                };

                foreach (var equipItem in _currentCharacter.EquipItems.Values)
                {
                    foreach (var texture in equipItem.ItemMesh.GetItemTextures(_currentCharacter.ConfigGuid))
                    {
                        var textureName = Helper.GetShaderTextureName(texture.Type);
                        if (textureName == null) continue;
                
                        mergeTextures[textureName] = texture.Texture;
                    }
                }
                
                MergeTexture(_tmpClothRenderShaderMaterial, _renderClothTexture, CharacterTexture, mergeTextures);
            }

            private void BuildArmorTexture()
            {
                var mergeTextures = new Dictionary<string, Texture2D>();
                foreach (var item in _currentCharacter.EquipItems)
                {
                    foreach (var mesh in item.Value.ItemMesh.GetItemMeshes(_currentCharacter.ConfigGuid, item.Key.IsAdditionalSlot()))
                    {
                        var textureName = Helper.GetShaderTextureName(mesh.MeshType);
                        if (textureName == null) continue;

                        mergeTextures[textureName] = mesh.Texture;
                    }
                }

                MergeTexture(_tmpArmorRenderShaderMaterial, _renderArmorTexture, ArmorTexture, mergeTextures);
            }

            private void MergeTexture(Material shaderMaterial, RenderTexture renderTexture, Texture2D resultTexture, Dictionary<string, Texture2D> mergeTextures)
            {
                _mergeTextureService.MergeTextures(shaderMaterial, renderTexture, mergeTextures);

                RenderTexture.active = renderTexture;
                resultTexture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
                resultTexture.Apply();
            }

            private void UpdateTexturesAndMeshes()
            {
                UpdateModelRenderers();
                UpdateArmorRenderers();
                UpdateFaceMeshRenders();
            }

            private void UpdateArmorRenderers()
            {
                var hidedMeshes = new List<MeshType>();
                foreach (var equipItem in _currentCharacter.EquipItems.Values)
                {
                    if (equipItem.Data.hidedMeshTypes.Length > 0) hidedMeshes.AddRange(equipItem.Data.hidedMeshTypes);
                }

                Dictionary<MaterialType, Material> characterMaterials;
                if (!_characterMaterials.TryGetValue(_currentCharacter.Guid, out characterMaterials)) return;

                var armorMaterial = characterMaterials[MaterialType.Armor];
                armorMaterial.mainTexture = ArmorTexture;

                foreach (var item in _currentCharacter.EquipItems.Values)
                {
                    var meshInfo = GetItemMeshes(item.Guid);
                    foreach (var meshItem in meshInfo.gameObjects)
                    {
                        foreach (var meshRenderer in meshItem.GetComponentsInChildren<MeshRenderer>())
                        {
                            meshRenderer.material = armorMaterial;
                        }

                        foreach (var particle in meshItem.GetComponentsInChildren<ParticleSystem>())
                        {
                            var itemRenderer = particle.GetComponent<Renderer>();
                            if (itemRenderer != null) itemRenderer.material.shader = _particleShader;
                        }
                        meshItem.SetActive(!hidedMeshes.Contains(meshInfo.meshType));
                    }
                }

                if (armorRawImage != null)
                {
                    armorRawImage.texture = ArmorTexture;
                    armorRawImage.enabled = true;
                }
            }

            private void UpdateFaceMeshRenders()
            {
                var hidedMeshes = new List<MeshType>();
                foreach (var equipItem in _currentCharacter.EquipItems.Values)
                    if (equipItem.Data.hidedMeshTypes.Length > 0) hidedMeshes.AddRange(equipItem.Data.hidedMeshTypes);

                foreach (var faceMesh in _currentCharacter.FaceMeshItems.Values)
                {
                    faceMesh.MeshInstance.SetActive(!hidedMeshes.Contains(faceMesh.MeshType));
                    if (faceMesh.PreviewMeshInstance != null) faceMesh.PreviewMeshInstance.SetActive(!hidedMeshes.Contains(faceMesh.MeshType));

                    // todo start
                    faceMeshMaterial.mainTexture = _currentCharacter.FaceMeshTexture;
                   
                    foreach (var meshRenderer in faceMesh.MeshInstance.GetComponentsInChildren<MeshRenderer>())
                         meshRenderer.material = faceMeshMaterial;

                    if (faceMesh.PreviewMeshInstance != null)
                    {
                        foreach (var meshRenderer in faceMesh.PreviewMeshInstance.GetComponentsInChildren<MeshRenderer>())
                            meshRenderer.material = faceMeshMaterial;
                    }
                    // todo end

                }
            }

            private void UpdateModelRenderers()
            {
                Dictionary<MaterialType, Material> chMaterials;
                if (_characterMaterials.TryGetValue(_currentCharacter.Guid, out chMaterials))
                {
                    chMaterials[MaterialType.Skin].mainTexture = CharacterTexture;
                }

//                foreach (var render in _modelRenders)
//                    render.material.mainTexture = CharacterTexture;

                if (skinRawImage != null) skinRawImage.texture = CharacterTexture;

                UpdatePanthRenders();
                UpdateCloakRenders();
            }

            private void UpdatePanthRenders()
            {
                foreach (var equipItem in _currentCharacter.EquipItems.Values)
                {
                    if (equipItem.ItemType != EquipItemType.Pants) continue;

                    switch (equipItem.ItemSubType)
                    {
                        case EquipItemSubType.LongRobe:
                            foreach (var render in _shortRobeRenders)
                                render.gameObject.SetActive(false);

                            foreach (var render in _longRobeRenders)
                            {
                                render.material.mainTexture = CharacterTexture;
                                render.gameObject.SetActive(true);
                            }

                            break;
                        case EquipItemSubType.ShortRobe:
                            foreach (var render in _longRobeRenders)
                                render.gameObject.SetActive(false);

                            foreach (var render in _shortRobeRenders)
                            {
                                render.material.mainTexture = CharacterTexture;
                                render.gameObject.SetActive(true);
                            }

                            break;
                        default:
                            foreach (var render in _longRobeRenders)
                                render.gameObject.SetActive(false);
                            foreach (var render in _shortRobeRenders)
                                render.gameObject.SetActive(false);
                            break;
                    }

                    return;
                }

                //If beard not equip, remove Robe
                foreach (var render in _longRobeRenders)
                    render.gameObject.SetActive(false);
                foreach (var render in _shortRobeRenders)
                    render.gameObject.SetActive(false);
            }

            private void UpdateCloakRenders()
            {
                _cloakTextures[_currentCharacter.Guid] = null;
                foreach (var equipItem in _currentCharacter.EquipItems.Values)
                {
                    if (equipItem.ItemType != EquipItemType.Cloak) continue;
                    foreach (var texture in equipItem.ItemMesh.GetItemTextures(_currentCharacter.ConfigGuid))
                    {
                        if (texture.Type != TextureType.Cloak) continue;
                        _cloakTextures[_currentCharacter.Guid] = texture.Texture;
                        break;
                    }
                }

                foreach (var cloakRender in _cloakRenders)
                {
                    cloakRender.gameObject.SetActive(CloakTexture != null);
                    if (CloakTexture != null) cloakRender.material.mainTexture = CloakTexture;
                }
            }

          
        }
    }
}