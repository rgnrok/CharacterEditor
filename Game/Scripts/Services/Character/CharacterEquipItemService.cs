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
        private readonly Dictionary<string, Texture2D> _characterTextures;
        private readonly Dictionary<string, Texture2D> _armorTextures;

        private Character _currentCharacter;
        private readonly Dictionary<string, Character> _characters = new Dictionary<string, Character>();

        private readonly List<Renderer> _modelRenders = new List<Renderer>();
        private readonly List<Renderer> _shortRobeRenders = new List<Renderer>();
        private readonly List<Renderer> _longRobeRenders = new List<Renderer>();
        private readonly List<Renderer> _cloakRenders = new List<Renderer>();

        private readonly List<Renderer> _previewModelRenders = new List<Renderer>();
        private readonly List<Renderer> _previewShortRobeRenders = new List<Renderer>();
        private readonly List<Renderer> _previewLongRobeRenders = new List<Renderer>();
        private readonly List<Renderer> _previewCloakRenders = new List<Renderer>();

        private readonly Dictionary<string, Dictionary<string, Dictionary<MeshType, List<GameObject>>>> _meshInstances;

        // Queue for destroy item gameObjects and unload bundle info
        private readonly List<EquipItem> _unEquipItemsQueue = new List<EquipItem>();

        // Queue for destroy only item gameObjects, without unload bundle info
        private readonly List<GameObject> _destroyMeshQueue = new List<GameObject>();


        private Material _modelMaterial;
        private Material _previewMaterial;

        private Material _tmpArmorRenderShaderMaterial;
        private Material _tmpClothRenderShaderMaterial;
        private readonly RenderTexture _renderClothTexture;
        private readonly RenderTexture _renderArmorTexture;

        private readonly IMergeTextureService _mergeTextureService;
        private readonly ILoaderService _loaderService;
        private readonly Dictionary<string, Dictionary<MaterialType, Material>> _characterMaterials;

        public event Action<EquipItem> OnEquip;
        public event Action<EquipItem> OnUnEquip;
        public event Action OnTexturesChanged;

        public CharacterEquipItemService(IMergeTextureService mergeTextureService, ILoaderService loaderService)
        {
            _mergeTextureService = mergeTextureService;
            _loaderService = loaderService;
       
            _armorTextures = new Dictionary<string, Texture2D>();
            _characterTextures = new Dictionary<string, Texture2D>();
            _characterMaterials = new Dictionary<string, Dictionary<MaterialType, Material>>();

            _meshInstances = new Dictionary<string, Dictionary<string, Dictionary<MeshType, List<GameObject>>>>();
            _renderArmorTexture = new RenderTexture(Constants.ARMOR_MESHES_ATLAS_SIZE,
                Constants.ARMOR_MESHES_ATLAS_SIZE, 0, RenderTextureFormat.ARGB32);
            _renderClothTexture = new RenderTexture(Constants.SKIN_TEXTURE_ATLAS_SIZE,
                Constants.SKIN_TEXTURE_ATLAS_SIZE, 0, RenderTextureFormat.ARGB32);
        }

        public async Task LoadMaterials()
        {
            var armorRenderShaderMaterial = await _loaderService.MaterialLoader.LoadByPath(AssetsConstants.ArmorMergeMaterialPathKey);
            var clothRenderShaderMaterial = await _loaderService.MaterialLoader.LoadByPath(AssetsConstants.ClothMergeMaterialPathKey);
            _modelMaterial = await _loaderService.MaterialLoader.LoadByPath(AssetsConstants.ModelMaterialPathKey);
            _previewMaterial = await _loaderService.MaterialLoader.LoadByPath(AssetsConstants.PreviewMaterialPathKey);

            _tmpArmorRenderShaderMaterial = new Material(armorRenderShaderMaterial);
            _tmpClothRenderShaderMaterial = new Material(clothRenderShaderMaterial);
        }

        public Texture2D GetCurrentCharacterTexture()
        {
            if (_currentCharacter == null) return null;
            return GetCharacterTexture(_currentCharacter.Guid);
        }

        public Texture2D GetCurrentCharacterArmorTexture()
        {
            if (_currentCharacter == null) return null;
            return GetCharacterArmorTexture(_currentCharacter.Guid);
        }

        public void CleanUp()
        {
            _currentCharacter = null;
            foreach (var character in _characters.Values)
                character.OnUnEquipItem -= UnEquipItemHandler;

            _characters.Clear();

            _meshInstances.Clear();
            _characterMaterials.Clear();
        }

        public void SetupCharacter(Character character)
        {
            if (_currentCharacter?.Guid == character.Guid) return;
            _currentCharacter = character;

            if (!_characters.ContainsKey(character.Guid))
            {
                character.OnUnEquipItem += UnEquipItemHandler;

                SetupCharacterRenders(character);
                UpdateCharacterMaterials(character);

                _meshInstances[character.Guid] = new Dictionary<string, Dictionary<MeshType, List<GameObject>>>(10);
                _characters[character.Guid] = character;
            }

            OnTexturesChanged?.Invoke();

        }

        public bool CanEquip(EquipItem item)
        {
            return CanEquip(_currentCharacter, item);
        }

        private bool CanEquip(Character character, EquipItem item)
        {
            return true; // todo check character parameters, class, and etc
        }

        public void UnEquipItem(EquipItem item)
        {
            UnEquipItem(_currentCharacter, item);
        }

        public async void EquipItem(EquipItem item, EquipItemSlot slotType = EquipItemSlot.Undefined)
        {
            await EquipItem(_currentCharacter, item, slotType);
        }

        public async Task EquipItems(Character character, Dictionary<EquipItemSlot, EquipItem> equipItems)
        {
            foreach (var itemPair in equipItems)
                await EquipItemInner(character, itemPair.Value, itemPair.Key);

            BuildTextures(character);
        }

        public async void SwapEquippedItem(EquipItemSlot slotType1, EquipItemSlot slotType2)
        {
            await SwapEquippedItem(_currentCharacter, slotType1, slotType2);
        }

        private void UnEquipItem(Character character, EquipItem item)
        {
            if (!character.UnEquipItem(item)) return;

            BuildTextures(character);
            FireUnEquipItem(character, item);
        }

        private async Task EquipItem(Character character, EquipItem item, EquipItemSlot slotType = EquipItemSlot.Undefined)
        {
            await EquipItemInner(character, item, slotType);
            BuildTextures(character);
        }

        private async Task SwapEquippedItem(Character character, EquipItemSlot slotType1, EquipItemSlot slotType2)
        {
            if (!character.EquipItems.TryGetValue(slotType1, out var item1)
                || !character.EquipItems.TryGetValue(slotType2, out var item2)) return;

            var meshInfo1 = GetItemInstancedMeshes(character, item1.Guid);
            var meshInfo2 = GetItemInstancedMeshes(character,item2.Guid);
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
            await item1.ItemMesh.LoadTexturesAndMeshes(character.ConfigGuid, slotType2.IsAdditionalSlot());
            await item2.ItemMesh.LoadTexturesAndMeshes(character.ConfigGuid, slotType1.IsAdditionalSlot());

            character.SwapItems(slotType1, slotType2);

            InstantiateMesh(character, item1, slotType2);
            InstantiateMesh(character, item2, slotType1);
            BuildTextures(character);
        }

        private void SetupCharacterRenders(Character character)
        {
            _modelRenders.Clear();
            _previewModelRenders.Clear();
            _modelRenders.AddRange(character.GameObjectData.SkinMeshes);
            if (character.GameObjectData.PreviewSkinMeshes != null)
                _previewModelRenders.AddRange(character.GameObjectData.PreviewSkinMeshes);

            _shortRobeRenders.Clear();
            _previewShortRobeRenders.Clear();
            _shortRobeRenders.AddRange(character.GameObjectData.ShortRobeMeshes);
            if (character.GameObjectData.PreviewShortRobeMeshes != null)
                _previewShortRobeRenders.AddRange(character.GameObjectData.PreviewShortRobeMeshes);

            _longRobeRenders.Clear();
            _previewLongRobeRenders.Clear();
            _longRobeRenders.AddRange(character.GameObjectData.LongRobeMeshes);
            if (character.GameObjectData.PreviewLongRobeMeshes != null)
                _previewLongRobeRenders.AddRange(character.GameObjectData.PreviewLongRobeMeshes);

            _cloakRenders.Clear();
            _previewCloakRenders.Clear();
            _cloakRenders.AddRange(character.GameObjectData.CloakMeshes);
            if (character.GameObjectData.PreviewCloakMeshes != null)
                _previewCloakRenders.AddRange(character.GameObjectData.PreviewCloakMeshes);
        }

        private void UpdateCharacterMaterials(Character character)
        {
            if (_characterMaterials.TryGetValue(character.Guid, out var chMaterials)) return;

            chMaterials = new Dictionary<MaterialType, Material>(EnumComparer.MaterialType)
            {
                {MaterialType.Skin, new Material(_modelMaterial)},
                {MaterialType.Face, new Material(_modelMaterial)},
                {MaterialType.Cloak, new Material(_modelMaterial)},
                {MaterialType.Armor, new Material(_modelMaterial)},
                {MaterialType.PreviewSkin, new Material(_previewMaterial)},
                {MaterialType.PreviewFace, new Material(_previewMaterial)},
                {MaterialType.PreviewCloak, new Material(_previewMaterial)},
                {MaterialType.PreviewArmor, new Material(_previewMaterial)},
            };
            _characterMaterials[character.Guid] = chMaterials;

            foreach (var ren in _modelRenders) ren.material = chMaterials[MaterialType.Skin];
            foreach (var ren in _shortRobeRenders) ren.material = chMaterials[MaterialType.Skin];
            foreach (var ren in _longRobeRenders) ren.material = chMaterials[MaterialType.Skin];
            foreach (var ren in _cloakRenders) ren.material = chMaterials[MaterialType.Cloak];

            foreach (var ren in _previewModelRenders) ren.material = chMaterials[MaterialType.PreviewSkin];
            foreach (var ren in _previewShortRobeRenders) ren.material = chMaterials[MaterialType.PreviewSkin];
            foreach (var ren in _previewLongRobeRenders) ren.material = chMaterials[MaterialType.PreviewSkin];
            foreach (var ren in _previewCloakRenders) ren.material = chMaterials[MaterialType.PreviewCloak];

            chMaterials[MaterialType.Face].mainTexture = character.FaceMeshTexture;
            chMaterials[MaterialType.PreviewFace].mainTexture = character.FaceMeshTexture;
            
            foreach (var faceMesh in character.FaceMeshItems.Values)
            {
                foreach (var meshRenderer in faceMesh.MeshInstance.GetComponentsInChildren<MeshRenderer>())
                    meshRenderer.material = chMaterials[MaterialType.Face];

                if (faceMesh.PreviewMeshInstance != null)
                {
                    foreach (var meshRenderer in faceMesh.PreviewMeshInstance.GetComponentsInChildren<MeshRenderer>())
                        meshRenderer.material = chMaterials[MaterialType.PreviewFace];
                }
            }
        }

        // todo refectorig!
        private void DestroyUnEquipMeshes(Character character)
        {
            foreach (var item in _unEquipItemsQueue)
            {
                var meshMap = GetItemInstancedMeshes(character, item.Guid);
                if (meshMap == null) continue;

                foreach (var meshes in meshMap.Values)
                {
                    foreach (var mesh in meshes)
                        Object.Destroy(mesh);
                    meshes.Clear();
                }

                item.ItemMesh.UnloadTexturesAndMesh(character.ConfigGuid);
            }

            _unEquipItemsQueue.Clear();

            foreach (var go in _destroyMeshQueue)
                Object.Destroy(go);
            _destroyMeshQueue.Clear();

        }


        private async Task EquipItemInner(Character character, EquipItem equipItem, EquipItemSlot slotType)
        {
            if (character.IsEquip(equipItem))
            {
                character.UnEquipItem(equipItem);
                FireUnEquipItem(character, equipItem);
                return;
            }

            slotType = character.EquipItem(equipItem, slotType);
            if (slotType == EquipItemSlot.Undefined) return;

            await equipItem.ItemMesh.LoadTexturesAndMeshes(character.ConfigGuid, slotType.IsAdditionalSlot());

            InstantiateMesh(character, equipItem, slotType);
            FireEquipItem(character, equipItem);
        }

        private Dictionary<MeshType, List<GameObject>> GetItemInstancedMeshes(Character character, string itemGuid)
        {
            if (!_meshInstances.TryGetValue(character.Guid, out var items)
                || !items.TryGetValue(itemGuid, out var meshInstance)) return null;

            return meshInstance;
        }

        private void InstantiateMesh(Character character, EquipItem item, EquipItemSlot slot)
        {
            if (!_meshInstances.TryGetValue(character.Guid, out var characterCache)) return;

            var meshes = item.ItemMesh.GetItemMeshes(character.ConfigGuid, slot.IsAdditionalSlot());
            var handMeshType = Helper.GetHandMeshTypeBySlot(slot);

            if (!characterCache.TryGetValue(item.Guid, out var itemCache))
                characterCache[item.Guid] = itemCache = new Dictionary<MeshType, List<GameObject>>(meshes.Length);

            foreach (var mesh in meshes)
            {
                if (!itemCache.TryGetValue(mesh.MeshType, out var goCache))
                    itemCache[mesh.MeshType] = goCache = new List<GameObject>(2);

                var boneMeshType = handMeshType != MeshType.Undefined ? handMeshType : mesh.MeshType;

                if (character.GameObjectData.meshBones.TryGetValue(boneMeshType, out var bone))
                {
                    var meshInstance = mesh.InstantiateMesh(bone, material: _characterMaterials[character.Guid][MaterialType.Armor]);
                    goCache.Add(meshInstance);
                }

                if (character.GameObjectData.previewMeshBones != null
                    && character.GameObjectData.previewMeshBones.TryGetValue(boneMeshType, out var previewBone))
                {
                    var meshInstance = mesh.InstantiateMesh(previewBone, Constants.LAYER_CHARACTER_PREVIEW, material: _characterMaterials[character.Guid][MaterialType.PreviewArmor]); 
                    goCache.Add(meshInstance);
                }
            }
        }


        private void UnEquipItemHandler(EquipItem item)
        {
            _unEquipItemsQueue.Add(item);
        }

        private void BuildTextures(Character character)
        {
            MergeClothTextures(character);
            BuildArmorTexture(character);

            DestroyUnEquipMeshes(character);
            UpdateTexturesAndMeshes(character);

            // OnTexturesChanged?.Invoke();
        }

        private void MergeClothTextures(Character character)
        {
            var mergeTextures = new Dictionary<string, Texture2D>(character.EquipItems.Count)
            {
                ["_SkinTex"] = character.Texture
            };

            foreach (var equipItem in character.EquipItems.Values)
            {
                foreach (var texture in equipItem.ItemMesh.GetItemTextures(character.ConfigGuid))
                {
                    var textureName = Helper.GetShaderTextureName(texture.Type);
                    if (textureName == null) continue;

                    mergeTextures[textureName] = texture.Texture;
                }
            }

            MergeTexture(_tmpClothRenderShaderMaterial, _renderClothTexture, GetCharacterTexture(character.Guid), mergeTextures);
        }

        private void BuildArmorTexture(Character character)
        {
            var mergeTextures = new Dictionary<string, Texture2D>();
            foreach (var item in character.EquipItems)
            {
                foreach (var mesh in item.Value.ItemMesh.GetItemMeshes(character.ConfigGuid,
                    item.Key.IsAdditionalSlot()))
                {
                    var textureName = Helper.GetShaderTextureName(mesh.MeshType);
                    if (textureName == null) continue;

                    mergeTextures[textureName] = mesh.Texture;
                }
            }

            MergeTexture(_tmpArmorRenderShaderMaterial, _renderArmorTexture, GetCharacterArmorTexture(character.Guid), mergeTextures);
        }

        private void MergeTexture(Material shaderMaterial, RenderTexture renderTexture, Texture2D resultTexture, Dictionary<string, Texture2D> mergeTextures)
        {
            _mergeTextureService.MergeTextures(shaderMaterial, renderTexture, mergeTextures);

            RenderTexture.active = renderTexture;
            resultTexture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            resultTexture.Apply();
        }

        private void UpdateTexturesAndMeshes(Character character)
        {
            UpdateModelRenders(character);
            UpdateArmorRenders(character);
            UpdateFaceMeshRenders(character);
        }

        private void UpdateArmorRenders(Character character)
        {
            if (!_characterMaterials.TryGetValue(character.Guid, out var characterMaterials)) return;

            characterMaterials[MaterialType.Armor].mainTexture = GetCharacterArmorTexture(character.Guid);
            characterMaterials[MaterialType.PreviewArmor].mainTexture = GetCharacterArmorTexture(character.Guid);

            var hidedMeshTypes = new List<MeshType>();
            foreach (var equipItem in character.EquipItems.Values)
            {
                if (equipItem.Data.hidedMeshTypes.Length == 0) continue;
                hidedMeshTypes.AddRange(equipItem.Data.hidedMeshTypes);
            }

            foreach (var item in character.EquipItems.Values)
            {
                var itemMeshes = GetItemInstancedMeshes(character, item.Guid);
                if (itemMeshes == null) continue;

                foreach (var meshItemPair in itemMeshes)
                {
                    foreach (var meshItem in meshItemPair.Value)
                    {
                        meshItem.SetActive(!hidedMeshTypes.Contains(meshItemPair.Key));
                    }
                }
            }
        }

        private void UpdateFaceMeshRenders(Character character)
        {
            var hidedMeshes = new List<MeshType>();
            foreach (var equipItem in character.EquipItems.Values)
                if (equipItem.Data.hidedMeshTypes.Length > 0)
                    hidedMeshes.AddRange(equipItem.Data.hidedMeshTypes);

            foreach (var faceMesh in character.FaceMeshItems.Values)
            {
                var isVisible = !hidedMeshes.Contains(faceMesh.MeshType);
                faceMesh.MeshInstance.SetActive(isVisible);
                faceMesh.PreviewMeshInstance?.SetActive(isVisible);
            }
        }

        private void UpdateModelRenders(Character character)
        {
            if (!_characterMaterials.TryGetValue(character.Guid, out var chMaterials)) return;
            var characterTexture = GetCharacterTexture(character.Guid);
            chMaterials[MaterialType.Skin].mainTexture = characterTexture;
            chMaterials[MaterialType.PreviewSkin].mainTexture = characterTexture;

            UpdatePantsVisible(character);
            UpdateCloakRenders(character);
        }

        private void UpdatePantsVisible(Character character)
        {
            var longRobeVisible = false;
            var shortRobeVisible = false;

            if (character.EquipItems.TryGetValue(EquipItemSlot.Pants, out var pantsItem))
            {
                longRobeVisible = pantsItem.ItemSubType == EquipItemSubType.LongRobe;
                shortRobeVisible = pantsItem.ItemSubType == EquipItemSubType.ShortRobe;
            }

            foreach (var render in _longRobeRenders)
                render.gameObject.SetActive(longRobeVisible);
            foreach (var render in _shortRobeRenders)
                render.gameObject.SetActive(shortRobeVisible);
        }

        private void UpdateCloakRenders(Character character)
        {
            var isCloakVisible = false;
            if (character.EquipItems.TryGetValue(EquipItemSlot.Cloak, out var cloakItem))
            {
                isCloakVisible = true;

                foreach (var texture in cloakItem.ItemMesh.GetItemTextures(character.ConfigGuid))
                {
                    if (texture.Type != TextureType.Cloak) continue;
                    _characterMaterials[character.Guid][MaterialType.Cloak].mainTexture = texture.Texture;
                    _characterMaterials[character.Guid][MaterialType.PreviewCloak].mainTexture = texture.Texture;
                    break;
                }
            }

            foreach (var cloakRender in _cloakRenders)
                cloakRender.gameObject.SetActive(isCloakVisible);
            foreach (var cloakRender in _previewCloakRenders)
                cloakRender.gameObject.SetActive(isCloakVisible);
        }

        private Texture2D GetCharacterTexture(string chGuid)
        {
            if (!_characterTextures.ContainsKey(chGuid))
                _characterTextures[chGuid] = Helper.CreateGameMergeTexture(Constants.SKIN_TEXTURE_ATLAS_SIZE);

            return _characterTextures[chGuid];
        }

        private Texture2D GetCharacterArmorTexture(string chGuid)
        {
            if (!_armorTextures.ContainsKey(chGuid))
                _armorTextures[chGuid] = Helper.CreateGameMergeTexture(Constants.ARMOR_MESHES_ATLAS_SIZE);

            return _armorTextures[chGuid];
        }

        private void FireEquipItem(Character character, EquipItem equipItem)
        {
            if (character.Guid != _currentCharacter.Guid) return;
            OnEquip?.Invoke(equipItem);
        }

        private void FireUnEquipItem(Character character, EquipItem equipItem)
        {
            if (character.Guid != _currentCharacter.Guid) return;
            OnUnEquip?.Invoke(equipItem);
        }
    }
}
