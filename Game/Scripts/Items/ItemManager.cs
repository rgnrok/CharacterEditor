using System;
using System.Collections;
using System.Collections.Generic;
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
            [SerializeField] private RenderTexture renderClothTexture;
            [SerializeField] private Material clothRenderShaderMaterial;


            private Dictionary<string, Texture2D> _characterTextures;
            public Texture2D CharacterTexture
            {
                get
                {
                    if (!_characterTextures.ContainsKey(_currentCharacter.guid))
                        _characterTextures[_currentCharacter.guid] = new Texture2D(Constants.SKIN_TEXTURE_ATLAS_SIZE, Constants.SKIN_TEXTURE_ATLAS_SIZE, TextureFormat.RGB24, false);

                    return _characterTextures[_currentCharacter.guid];
                }
            }

            private Dictionary<string, Texture2D> _armorTextures;
            public Texture2D ArmorTexture
            {
                get
                {
                    if (!_armorTextures.ContainsKey(_currentCharacter.guid))
                        _armorTextures[_currentCharacter.guid] = new Texture2D(Constants.ARMOR_MESHES_ATLAS_SIZE, Constants.ARMOR_MESHES_ATLAS_SIZE, TextureFormat.RGB24, false);

                    return _armorTextures[_currentCharacter.guid];
                }
            }

            private Dictionary<string, Texture2D> _cloakTextures;
            public Texture2D CloakTexture
            {
                get
                {
                    if (!_cloakTextures.ContainsKey(_currentCharacter.guid))
                        _cloakTextures[_currentCharacter.guid] = null;

                    return _cloakTextures[_currentCharacter.guid];
                }
            }
            public bool IsReady { get; private set; }

            private Character _currentCharacter;
            private readonly List<Renderer> _modelRenders = new List<Renderer>();
            private readonly List<Renderer> _shortRobeRenders = new List<Renderer>();
            private readonly List<Renderer> _longRobeRenders = new List<Renderer>();
            private readonly List<Renderer> _cloakRenders = new List<Renderer>();

            private TextureShaderType _currentShaderType;
            private Shader _particleShader;

            private Coroutine _buildCoroutine;
            private Coroutine _clothCoroutine;
            private Coroutine _armorCoroutine;

         
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


            private void Awake()
            {
                if (Instance != null) Destroy(gameObject);
                Instance = this;
                IsReady = true;

                _armorTextures = new Dictionary<string, Texture2D>();
                _characterTextures = new Dictionary<string, Texture2D>();
                _cloakTextures = new Dictionary<string, Texture2D>();
                _characterMaterials = new Dictionary<string, Dictionary<MaterialType, Material>>();

                _meshInstances = new Dictionary<string, Dictionary<string, MeshInstanceInfo>>();
                _particleShader = Shader.Find("Particles/Additive (Soft)");

                _mergeTextureService = new MergeTextureService();
            }


            /*
            * Change Character. Update textures and skin meshes
            */
            public void SetCharacter(Character character)
            {
                if (_currentCharacter != null)
                {
                    if (_currentCharacter.guid == character.guid) return;
                    _currentCharacter.OnUnequipItem -= UnequipItemHandler;
                }

                _currentCharacter = character;
                _currentCharacter.OnUnequipItem += UnequipItemHandler;

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
                if (!_characterMaterials.TryGetValue(_currentCharacter.guid, out chMaterials))
                {
                    chMaterials = new Dictionary<MaterialType, Material>
                    {
                        {MaterialType.Skin, new Material(defaultMaterial)},
                        {MaterialType.Face, new Material(defaultMaterial)},
                        {MaterialType.Cloak, new Material(defaultMaterial)},
                        {MaterialType.Armor, new Material(defaultMaterial)},
                    };
                    _characterMaterials[_currentCharacter.guid] = chMaterials;
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

                    item.ItemMesh.UnloadTexturesAndMesh(_currentCharacter.configGuid);
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

            public void EquipItem(EquipItem item, EquipItemSlot slotType = EquipItemSlot.Undefined)
            {
                StartCoroutine(EquipItemCoroutine(item, slotType, true));
            }

            public void EquipItems(Dictionary<EquipItemSlot, EquipItem> equipItems)
            {
                IsReady = false;
                StartCoroutine(EquipItemsCoroutine(equipItems));
            }

            public void SwapEquipedItem(EquipItemSlot slotType1, EquipItemSlot slotType2)
            {
                StartCoroutine(SwapEquipedItemCoroutine(slotType1, slotType2));
            }

            private IEnumerator SwapEquipedItemCoroutine(EquipItemSlot slotType1, EquipItemSlot slotType2)
            {
                EquipItem item1, item2;
                if (!_currentCharacter.EquipItems.TryGetValue(slotType1, out item1) || !_currentCharacter.EquipItems.TryGetValue(slotType2, out item2)) yield break;
                if (!_currentCharacter.IsEquip(item1) || !_currentCharacter.IsEquip(item2)) yield break;

                var meshInfo1 = GetItemMeshes(item1.Guid);
                var meshInfo2 = GetItemMeshes(item2.Guid);
                foreach (var go in meshInfo1.gameObjects)
                    _destroyMeshQueue.Add(go);

                foreach (var go in meshInfo2.gameObjects)
                    _destroyMeshQueue.Add(go);

                meshInfo1.gameObjects.Clear();
                meshInfo2.gameObjects.Clear();

                // Left and Right hand has different mesh models
                item1.ItemMesh.LoadTexturesAndMeshes(_currentCharacter.configGuid, slotType2);
                item2.ItemMesh.LoadTexturesAndMeshes(_currentCharacter.configGuid, slotType1);

                while (!item1.ItemMesh.IsReady(_currentCharacter.configGuid)) yield return null;
                while (!item2.ItemMesh.IsReady(_currentCharacter.configGuid)) yield return null;

                _currentCharacter.SwapItems(slotType1, slotType2);

                InstanceMesh(item1, slotType2);
                InstanceMesh(item2, slotType1);
                BuildTextures();
            }

            private IEnumerator EquipItemsCoroutine(Dictionary<EquipItemSlot, EquipItem> equipItems)
            {
                foreach (var itemPair in equipItems)
                    yield return StartCoroutine(EquipItemCoroutine(itemPair.Value, itemPair.Key, false));

                BuildTextures();
            }

            private IEnumerator EquipItemCoroutine(EquipItem equipItem, EquipItemSlot slotType, bool withUpdateTextures)
            {
                if (_currentCharacter.IsEquip(equipItem))
                {
                    _currentCharacter.UnEquipItem(equipItem);
                    if (withUpdateTextures) BuildTextures();
                    if (OnUnEquip != null) OnUnEquip(equipItem);
                    yield break;
                }

                slotType = _currentCharacter.EquipItem(equipItem, slotType);
                if (slotType == EquipItemSlot.Undefined) yield break;

                equipItem.ItemMesh.LoadTexturesAndMeshes(_currentCharacter.configGuid, slotType);
                while (!equipItem.ItemMesh.IsReady(_currentCharacter.configGuid)) yield return null;

                InstanceMesh(equipItem, slotType);
                if (withUpdateTextures) BuildTextures();
                if (OnEquip != null) OnEquip(equipItem);
            }
            
            private MeshInstanceInfo GetItemMeshes(string itemGuid)
            {
              
                var emptyDic = new MeshInstanceInfo(MeshType.Undefined, new List<GameObject>());
                if (!_meshInstances.ContainsKey(_currentCharacter.guid) ||
                    !_meshInstances[_currentCharacter.guid].ContainsKey(itemGuid)) return emptyDic;

                return _meshInstances[_currentCharacter.guid][itemGuid];
            }

            private void InstanceMesh(string itemGuid, ItemMesh mesh, Transform bone, int layer = 0)
            {
                if (!_meshInstances.ContainsKey(_currentCharacter.guid))
                    _meshInstances[_currentCharacter.guid] = new Dictionary<string, MeshInstanceInfo>();

                if (!_meshInstances[_currentCharacter.guid].ContainsKey(itemGuid))
                    _meshInstances[_currentCharacter.guid][itemGuid] = new MeshInstanceInfo(mesh.MeshType, new List<GameObject>());

                _meshInstances[_currentCharacter.guid][itemGuid].gameObjects.Add(mesh.InstanceMesh(bone, layer, false, false));//why disabled lod in old code ?
            }

            private void InstanceMesh(EquipItem item, EquipItemSlot slot)
            {
                var meshes = item.ItemMesh.GetItemMeshs(_currentCharacter.configGuid, slot);
                var meshType = Helper.GetHandMeshTypeBySlot(slot);

                foreach (var mesh in meshes)
                {
                    var boneMesh = meshType != MeshType.Undefined ? meshType : mesh.MeshType;

                    Transform bone;
                    if (_currentCharacter.GameObjectData.meshBones.TryGetValue(boneMesh, out bone))
                        InstanceMesh(item.Guid, mesh, bone);

                    if (_currentCharacter.GameObjectData.previewMeshBones != null)
                    {
                        Transform previewBone;
                        if (_currentCharacter.GameObjectData.previewMeshBones.TryGetValue(boneMesh, out previewBone))
                            InstanceMesh(item.Guid, mesh, previewBone, Constants.LAYER_CHARACTER_PREVIEW);
                    }
                }
            }

            private void UnequipItemHandler(EquipItem item)
            {
                _unequipItemsQueue.Add(item);
            }

            private void BuildTextures()
            {
                IsReady = false;

                if (_buildCoroutine != null) StopCoroutine(_buildCoroutine);
                if (_clothCoroutine != null) StopCoroutine(_clothCoroutine);
                if (_armorCoroutine != null) StopCoroutine(_armorCoroutine);

                _buildCoroutine = StartCoroutine(BuildTexturesCoroutine());
            }

            private IEnumerator BuildTexturesCoroutine()
            {
                yield return _clothCoroutine = StartCoroutine(MergeClothTextures());
                yield return _armorCoroutine = StartCoroutine(BuildArmorTexture());

                UpdateModelTextures();

                _buildCoroutine = null;
                IsReady = true;
            }
            
            /*
             * Combining the texture of the character
             */
            private IEnumerator MergeClothTextures()
            {
                var mergeTextures = new Dictionary<string, Texture2D>(_currentCharacter.EquipItems.Count)
                {
                    ["_SkinTex"] = _currentCharacter.Texture
                };

                foreach (var equipItem in _currentCharacter.EquipItems.Values)
                {
                    foreach (var texture in equipItem.ItemMesh.GetItemTextures(_currentCharacter.configGuid))
                    {
                        var textureName = texture.GetShaderTextureName();
                        if (textureName == null) continue;
                
                        while (!texture.IsReady) yield return null;
                        mergeTextures[textureName] = texture.Texture;
                    }
                }
                
                _mergeTextureService.MergeTextures(clothRenderShaderMaterial, renderClothTexture, mergeTextures);
                
                RenderTexture.active = renderClothTexture;
                CharacterTexture.ReadPixels(new Rect(0, 0, renderClothTexture.width, renderClothTexture.height), 0, 0);
                CharacterTexture.Apply();

                _clothCoroutine = null;
            }


            private IEnumerator BuildArmorTexture()
            {
                var meshes = new List<ItemMesh>();
                foreach (var item in _currentCharacter.EquipItems)
                {
                    foreach (var mesh in item.Value.ItemMesh.GetItemMeshs(_currentCharacter.configGuid, item.Key))
                    {
                        while (!mesh.IsReady) yield return null;
                        meshes.Add(mesh);
                    }
                }

                //Create empty atlas
                if (meshes.Count == 0)
                {
                    if (armorRawImage != null)
                    {
                        armorRawImage.enabled = false;
                        armorRawImage.texture = ArmorTexture;
                    }
                    yield break;
                }

                const int size = Constants.ARMOR_MESHES_ATLAS_SIZE;
                const int meshTextureSize = Constants.MESH_TEXTURE_SIZE;

                //Insert mesh textures in atlas
                foreach (var selectedMesh in meshes)
                {
                    var position = selectedMesh.AtlasPosition;
                    var x = meshTextureSize * (position % (size / meshTextureSize));
                    var y = (size - meshTextureSize) - (meshTextureSize * position / size) * meshTextureSize;

                    ArmorTexture.SetPixels32(x, y, meshTextureSize, meshTextureSize,
                        selectedMesh.Texture.GetPixels32());
                }

                ArmorTexture.Apply();
                _armorCoroutine = null;
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
                if (!_characterMaterials.TryGetValue(_currentCharacter.guid, out characterMaterials)) return;

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
                    faceMesh.MeshInstance.SetActive(!hidedMeshes.Contains(faceMesh.ItemMesh.MeshType));
                    if (faceMesh.PreviewMeshInstance != null) faceMesh.PreviewMeshInstance.SetActive(!hidedMeshes.Contains(faceMesh.ItemMesh.MeshType));

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
                if (_characterMaterials.TryGetValue(_currentCharacter.guid, out chMaterials))
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
                _cloakTextures[_currentCharacter.guid] = null;
                foreach (var equipItem in _currentCharacter.EquipItems.Values)
                {
                    if (equipItem.ItemType != EquipItemType.Cloak) continue;
                    foreach (var texture in equipItem.ItemMesh.GetItemTextures(_currentCharacter.configGuid))
                    {
                        if (texture.Type != TextureType.Cloak) continue;
                        _cloakTextures[_currentCharacter.guid] = texture.Texture;
                        break;
                    }
                }

                foreach (var cloakRender in _cloakRenders)
                {
                    cloakRender.gameObject.SetActive(CloakTexture != null);
                    if (CloakTexture != null) cloakRender.material.mainTexture = CloakTexture;
                }
            }

//            public void SetShader(TextureShaderType shader)
//            {
//                _currentShaderType = shader;
//
//                var materialInfo = GetShaderMaterial(shader);
//                if (materialInfo == null) return;
//
//                var material = materialInfo.skinMaterial;
//                material.mainTexture = CharacterTexture;
//                foreach (var render in _modelRenders)
//                    render.material = material;
//                foreach (var render in _longRobeRenders)
//                    render.material = material;
//                foreach (var render in _shortRobeRenders)
//                    render.material = material;
//
//                var cloakMaterial = materialInfo.cloakMaterial;
//                cloakMaterial.mainTexture = CloakTexture;
//                foreach (var render in _cloakRenders)
//                    render.material = cloakMaterial;
//
//                var armorMaterial = materialInfo.armorMeshMaterial;
//                armorMaterial.mainTexture = ArmorTexture;
//                foreach (var item in _currentCharacter.EquipItems.Values)
//                {
//                    var meshInfo = GetItemMeshes(item.Guid);
//                    foreach (var meshItem in meshInfo.gameObjects)
//                    {
//                        foreach (var meshRenderer in meshItem.GetComponentsInChildren<MeshRenderer>())
//                            meshRenderer.material = armorMaterial;
//                    }
//                }
//
//                var faceMaterial = materialInfo.faceMeshMaterial;
//                faceMaterial.mainTexture = _currentCharacter.FaceMeshTexture;
//                foreach (var item in _currentCharacter.FaceMeshItems.Values)
//                {
//                    foreach (var meshRenderer in item.MeshInstance.GetComponentsInChildren<MeshRenderer>())
//                        meshRenderer.material = faceMaterial;
//
//                    if (item.PreviewMeshInstance == null) continue;
//
//                    foreach (var meshRenderer in item.PreviewMeshInstance.GetComponentsInChildren<MeshRenderer>())
//                        meshRenderer.material = faceMaterial;
//                }
//            }

//            private MaterialInfo GetShaderMaterial(TextureShaderType shader)
//            {
//                foreach (var mat in Materials)
//                    if (mat.shader == shader) return mat;
//
//                return null;
//            }

          
        }
    }
}