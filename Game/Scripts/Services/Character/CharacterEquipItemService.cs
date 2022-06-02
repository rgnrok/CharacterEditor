using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CharacterEditor.CharacterInventory;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CharacterEditor.Services
{
    public class CharacterEquipItemService : ICharacterEquipItemService
    {
        private Dictionary<string, Texture2D> _characterTextures;

        public Texture2D CharacterTexture
        {
            get
            {
                if (!_characterTextures.ContainsKey(_currentCharacter.Guid))
                    _characterTextures[_currentCharacter.Guid] = Helper.CreateGameMergeTexture(Constants.SKIN_TEXTURE_ATLAS_SIZE);

                return _characterTextures[_currentCharacter.Guid];
            }
        }

        private Dictionary<string, Texture2D> _armorTextures;

        public Texture2D ArmorTexture
        {
            get
            {
                if (!_armorTextures.ContainsKey(_currentCharacter.Guid))
                    _armorTextures[_currentCharacter.Guid] = Helper.CreateGameMergeTexture(Constants.ARMOR_MESHES_ATLAS_SIZE);

                return _armorTextures[_currentCharacter.Guid];
            }
        }

        private Character _currentCharacter;
        private readonly List<Renderer> _modelRenders = new List<Renderer>();
        private readonly List<Renderer> _shortRobeRenders = new List<Renderer>();
        private readonly List<Renderer> _longRobeRenders = new List<Renderer>();
        private readonly List<Renderer> _cloakRenders = new List<Renderer>();

        private Shader _particleShader;

        private Dictionary<string, Dictionary<string, Dictionary<MeshType, List<GameObject>>>> _meshInstances;

        // Queue for destroy item gameobjects and unload bundle info
        private readonly List<EquipItem> _unEquipItemsQueue = new List<EquipItem>();

        // Queue for destroy only item gameobjects, without unload bundle info
        private readonly List<GameObject> _destroyMeshQueue = new List<GameObject>();


        private Material _defaultMaterial;
        private Material _tmpArmorRenderShaderMaterial;
        private Material _tmpClothRenderShaderMaterial;
        private RenderTexture _renderClothTexture;
        private RenderTexture _renderArmorTexture;

        private IMergeTextureService _mergeTextureService;


        private Dictionary<string, Dictionary<MaterialType, Material>> _characterMaterials;

        public event Action<EquipItem> OnEquip;
        public event Action<EquipItem> OnUnEquip;
        public event Action OnTexturesChanged;

        public CharacterEquipItemService(IMergeTextureService mergeTextureService, Material defaultMaterial, Material clothRenderShaderMaterial, Material armorRenderShaderMaterial)
        {
            _mergeTextureService = mergeTextureService;
            _defaultMaterial = defaultMaterial;
            _tmpArmorRenderShaderMaterial = new Material(armorRenderShaderMaterial);
            _tmpClothRenderShaderMaterial = new Material(clothRenderShaderMaterial);

            _armorTextures = new Dictionary<string, Texture2D>();
            _characterTextures = new Dictionary<string, Texture2D>();
            _characterMaterials = new Dictionary<string, Dictionary<MaterialType, Material>>();

            _meshInstances = new Dictionary<string, Dictionary<string, Dictionary<MeshType, List<GameObject>>>>();
            _particleShader = Shader.Find("Particles/Additive (Soft)"); //todo


            _renderArmorTexture = new RenderTexture(Constants.ARMOR_MESHES_ATLAS_SIZE,
                Constants.ARMOR_MESHES_ATLAS_SIZE, 0, RenderTextureFormat.ARGB32);
            _renderClothTexture = new RenderTexture(Constants.SKIN_TEXTURE_ATLAS_SIZE,
                Constants.SKIN_TEXTURE_ATLAS_SIZE, 0, RenderTextureFormat.ARGB32);

            // _saveLoadService = AllServices.Container.Single<ISaveLoadService>();
            // _saveLoadService.OnCharactersLoaded += OnCharactersLoadedHandler;
        }

        public void SetCharacter(Character character)
        {
            if (_currentCharacter?.Guid == character.Guid) return;

            if (_currentCharacter != null)
                _currentCharacter.OnUnEquipItem -= UnEquipItemHandler;

            _currentCharacter = character;
            _currentCharacter.OnUnEquipItem += UnEquipItemHandler;

            SetupCharacterRenders(character);
            UpdateCharacterMaterials();

            if (!_meshInstances.ContainsKey(_currentCharacter.Guid))
                _meshInstances[_currentCharacter.Guid] = new Dictionary<string, Dictionary<MeshType, List<GameObject>>>(10);

            OnTexturesChanged?.Invoke();
        }

        public bool CanEquip(EquipItem item)
        {
            return true; // todo check character parameters, class, and etc
        }

        public void UnEquipItem(EquipItem item)
        {
            if (!_currentCharacter.UnEquipItem(item)) return;

            BuildTextures();
            OnUnEquip?.Invoke(item);
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

        public async void SwapEquippedItem(EquipItemSlot slotType1, EquipItemSlot slotType2)
        {
            if (!_currentCharacter.EquipItems.TryGetValue(slotType1, out var item1)
                || !_currentCharacter.EquipItems.TryGetValue(slotType2, out var item2)) return;

            var meshInfo1 = GetItemInstancedMeshes(item1.Guid);
            var meshInfo2 = GetItemInstancedMeshes(item2.Guid);
            if (meshInfo1 == null || meshInfo2 == null) return;

            foreach (var meshes in meshInfo1.Values)
            {
                foreach (var go in meshes) _destroyMeshQueue.Add(go);
                meshes.Clear();
            }

            foreach (var meshes in meshInfo2.Values)
            {
                foreach (var go in meshes) _destroyMeshQueue.Add(go);
                meshes.Clear();
            }

            // Left and Right hand has different mesh models
            await item1.ItemMesh.LoadTexturesAndMeshes(_currentCharacter.ConfigGuid, slotType2.IsAdditionalSlot());
            await item2.ItemMesh.LoadTexturesAndMeshes(_currentCharacter.ConfigGuid, slotType1.IsAdditionalSlot());

            _currentCharacter.SwapItems(slotType1, slotType2);

            InstantiateMesh(item1, slotType2);
            InstantiateMesh(item2, slotType1);
            BuildTextures();
        }

        private void SetupCharacterRenders(Character character)
        {
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
        }

        private void UpdateCharacterMaterials()
        {
            if (_characterMaterials.TryGetValue(_currentCharacter.Guid, out var chMaterials)) return;

            chMaterials = new Dictionary<MaterialType, Material>(EnumComparer.MaterialType)
            {
                {MaterialType.Skin, new Material(_defaultMaterial)},
                {MaterialType.Face, new Material(_defaultMaterial)},
                {MaterialType.Cloak, new Material(_defaultMaterial)},
                {MaterialType.Armor, new Material(_defaultMaterial)},
            };
            _characterMaterials[_currentCharacter.Guid] = chMaterials;


            foreach (var ren in _modelRenders) ren.material = chMaterials[MaterialType.Skin];
            foreach (var ren in _shortRobeRenders) ren.material = chMaterials[MaterialType.Skin];
            foreach (var ren in _longRobeRenders) ren.material = chMaterials[MaterialType.Skin];
            foreach (var ren in _cloakRenders) ren.material = chMaterials[MaterialType.Cloak];

            var faceMeshMaterial = chMaterials[MaterialType.Face];
            foreach (var faceMesh in _currentCharacter.FaceMeshItems.Values)
            {
                faceMeshMaterial.mainTexture = _currentCharacter.FaceMeshTexture;

                foreach (var meshRenderer in faceMesh.MeshInstance.GetComponentsInChildren<MeshRenderer>())
                    meshRenderer.material = faceMeshMaterial;

                if (faceMesh.PreviewMeshInstance != null)
                {
                    foreach (var meshRenderer in faceMesh.PreviewMeshInstance.GetComponentsInChildren<MeshRenderer>())
                        meshRenderer.material = faceMeshMaterial;
                }
            }
        }

        // todo refectorig!

        private void DestroyUnEquipMeshes()
        {
            foreach (var item in _unEquipItemsQueue)
            {
                var meshMap = GetItemInstancedMeshes(item.Guid);
                if (meshMap == null) continue;

                foreach (var meshes in meshMap.Values)
                {
                    foreach (var mesh in meshes)
                        Object.Destroy(mesh);
                    meshes.Clear();
                }

                item.ItemMesh.UnloadTexturesAndMesh(_currentCharacter.ConfigGuid);
            }

            _unEquipItemsQueue.Clear();

            foreach (var go in _destroyMeshQueue)
                Object.Destroy(go);
            _destroyMeshQueue.Clear();

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

            InstantiateMesh(equipItem, slotType);
            OnEquip?.Invoke(equipItem);
        }

        private Dictionary<MeshType, List<GameObject>> GetItemInstancedMeshes(string itemGuid)
        {
            if (!_meshInstances.TryGetValue(_currentCharacter.Guid, out var items)
                || !items.TryGetValue(itemGuid, out var meshInstance)) return null;

            return meshInstance;
        }

        private void InstantiateMesh(EquipItem item, EquipItemSlot slot)
        {
            if (!_meshInstances.TryGetValue(_currentCharacter.Guid, out var characterCache)) return;

            var meshes = item.ItemMesh.GetItemMeshes(_currentCharacter.ConfigGuid, slot.IsAdditionalSlot());
            var handMeshType = Helper.GetHandMeshTypeBySlot(slot);

            if (!characterCache.TryGetValue(item.Guid, out var itemCache))
                characterCache[item.Guid] = itemCache = new Dictionary<MeshType, List<GameObject>>(meshes.Length);

            foreach (var mesh in meshes)
            {
                if (!itemCache.TryGetValue(mesh.MeshType, out var goCache))
                    itemCache[mesh.MeshType] = goCache = new List<GameObject>(2);

                var boneMeshType = handMeshType != MeshType.Undefined ? handMeshType : mesh.MeshType;

                if (_currentCharacter.GameObjectData.meshBones.TryGetValue(boneMeshType, out var bone))
                {
                    var meshInstance = mesh.InstantiateMesh(bone, 0, false); //why disabled lod in old code ?
                    goCache.Add(meshInstance);
                }

                if (_currentCharacter.GameObjectData.previewMeshBones != null
                    && _currentCharacter.GameObjectData.previewMeshBones.TryGetValue(boneMeshType, out var previewBone))
                {
                    var meshInstance =
                        mesh.InstantiateMesh(previewBone, Constants.LAYER_CHARACTER_PREVIEW,
                            false); //why disabled lod in old code ?
                    goCache.Add(meshInstance);
                }
            }
        }


        private void UnEquipItemHandler(EquipItem item)
        {
            _unEquipItemsQueue.Add(item);
        }

        private void BuildTextures()
        {
            MergeClothTextures();
            BuildArmorTexture();

            DestroyUnEquipMeshes();
            UpdateTexturesAndMeshes();

            // OnTexturesChanged?.Invoke();
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
                foreach (var mesh in item.Value.ItemMesh.GetItemMeshes(_currentCharacter.ConfigGuid,
                    item.Key.IsAdditionalSlot()))
                {
                    var textureName = Helper.GetShaderTextureName(mesh.MeshType);
                    if (textureName == null) continue;

                    mergeTextures[textureName] = mesh.Texture;
                }
            }

            MergeTexture(_tmpArmorRenderShaderMaterial, _renderArmorTexture, ArmorTexture, mergeTextures);
        }

        private void MergeTexture(Material shaderMaterial, RenderTexture renderTexture, Texture2D resultTexture,
            Dictionary<string, Texture2D> mergeTextures)
        {
            _mergeTextureService.MergeTextures(shaderMaterial, renderTexture, mergeTextures);

            RenderTexture.active = renderTexture;
            resultTexture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            resultTexture.Apply();
        }

        private void UpdateTexturesAndMeshes()
        {
            UpdateModelRenders();
            UpdateArmorRenders();
            UpdateFaceMeshRenders();
        }

        private void UpdateArmorRenders()
        {
            if (!_characterMaterials.TryGetValue(_currentCharacter.Guid, out var characterMaterials)) return;

            var armorMaterial = characterMaterials[MaterialType.Armor];
            armorMaterial.mainTexture = ArmorTexture;

            var hidedMeshTypes = new List<MeshType>();
            foreach (var equipItem in _currentCharacter.EquipItems.Values)
            {
                if (equipItem.Data.hidedMeshTypes.Length == 0) continue;
                hidedMeshTypes.AddRange(equipItem.Data.hidedMeshTypes);
            }

            foreach (var item in _currentCharacter.EquipItems.Values)
            {
                var itemMeshes = GetItemInstancedMeshes(item.Guid);
                if (itemMeshes == null) continue;

                foreach (var meshItemPair in itemMeshes)
                {
                    foreach (var meshItem in meshItemPair.Value)
                    {
                        foreach (var meshRenderer in meshItem.GetComponentsInChildren<MeshRenderer>())
                        {
                            var meshMaterials = meshRenderer.materials;
                            for (var i = 0; i < meshMaterials.Length; i++) meshMaterials[i] = armorMaterial;
                            meshRenderer.materials = meshMaterials;
                        }

                        // foreach (var particle in meshItem.GetComponentsInChildren<ParticleSystem>())
                        // {
                        //     var itemRenderer = particle.GetComponent<Renderer>();
                        //     if (itemRenderer != null) itemRenderer.material.shader = _particleShader;
                        // }

                        meshItem.SetActive(!hidedMeshTypes.Contains(meshItemPair.Key));
                    }
                }
            }
        }

        private void UpdateFaceMeshRenders()
        {
            var hidedMeshes = new List<MeshType>();
            foreach (var equipItem in _currentCharacter.EquipItems.Values)
                if (equipItem.Data.hidedMeshTypes.Length > 0)
                    hidedMeshes.AddRange(equipItem.Data.hidedMeshTypes);

            foreach (var faceMesh in _currentCharacter.FaceMeshItems.Values)
            {
                var isVisible = !hidedMeshes.Contains(faceMesh.MeshType);
                faceMesh.MeshInstance.SetActive(isVisible);
                faceMesh.PreviewMeshInstance?.SetActive(isVisible);
            }
        }

        private void UpdateModelRenders()
        {
            if (!_characterMaterials.TryGetValue(_currentCharacter.Guid, out var chMaterials)) return;
            chMaterials[MaterialType.Skin].mainTexture = CharacterTexture;

            //                foreach (var render in _modelRenders)
            //                    render.material.mainTexture = CharacterTexture;

            UpdatePantsVisible();
            UpdateCloakRenders();
        }

        private void UpdatePantsVisible()
        {
            var longRobeVisible = false;
            var shortRobeVisible = false;

            if (_currentCharacter.EquipItems.TryGetValue(EquipItemSlot.Pants, out var pantsItem))
            {
                longRobeVisible = pantsItem.ItemSubType == EquipItemSubType.LongRobe;
                shortRobeVisible = pantsItem.ItemSubType == EquipItemSubType.ShortRobe;
            }

            foreach (var render in _longRobeRenders)
                render.gameObject.SetActive(longRobeVisible);
            foreach (var render in _shortRobeRenders)
                render.gameObject.SetActive(shortRobeVisible);
        }

        private void UpdateCloakRenders()
        {
            var isCloakVisible = false;
            if (_currentCharacter.EquipItems.TryGetValue(EquipItemSlot.Cloak, out var cloakItem))
            {
                isCloakVisible = true;

                foreach (var texture in cloakItem.ItemMesh.GetItemTextures(_currentCharacter.ConfigGuid))
                {
                    if (texture.Type != TextureType.Cloak) continue;
                    _characterMaterials[_currentCharacter.Guid][MaterialType.Cloak].mainTexture = texture.Texture;
                    break;
                }
            }

            foreach (var cloakRender in _cloakRenders)
                cloakRender.gameObject.SetActive(isCloakVisible);
        }
    }
}
