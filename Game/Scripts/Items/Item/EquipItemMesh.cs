using System.Collections.Generic;
using StatSystem;

namespace CharacterEditor
{
    namespace CharacterInventory
    {
        public class EquipItemMesh
        {
            protected readonly Dictionary<string, ItemTexture[]> _itemTextures = new Dictionary<string, ItemTexture[]>();
            protected readonly Dictionary<string, ItemMesh[]> _originalItemMeshes = new Dictionary<string, ItemMesh[]>();
            protected readonly Dictionary<string, ItemMesh[]> _additionalItemMeshes = new Dictionary<string, ItemMesh[]>();
            protected readonly Dictionary<string, ItemMesh[]> _itemMeshes = new Dictionary<string, ItemMesh[]>();


            private readonly EquipItemData _equipItemData;
            public new EquipItemData Data
            {
                get
                {
                    return _equipItemData;
                }
            }

            public bool IsReady(string configGuid)
            {
                ItemTexture[] textures;
                if (_itemTextures.TryGetValue(configGuid, out textures))
                {
                    foreach (var texture in textures)
                    {
                        if (!texture.IsReady) return false;
                    }
                }

                ItemMesh[] meshes;
                if (_itemMeshes.TryGetValue(configGuid, out meshes))
                {
                    foreach (var mesh in meshes)
                    {
                        if (!mesh.IsReady) return false;
                    }
                }

                return true;
            }

            public EquipItemMesh(EquipItemData itemData, ITextureLoader textureLoader, IMeshLoader meshLoader)
            {
                _equipItemData = itemData;

                foreach (var configData in Data.configsItems)
                {
                    _itemTextures[configData.configGuid] = new ItemTexture[configData.textures.Length];
                    for (var i = 0; i < configData.textures.Length; i++)
                    {
                        var textureData = configData.textures[i];
                        _itemTextures[configData.configGuid][i] = ClothTextureFactory.Create(textureData.textureType, textureLoader, textureData.textureBundlePath);
                    }

                    _originalItemMeshes[configData.configGuid] = new ItemMesh[configData.models.Length];
                    _additionalItemMeshes[configData.configGuid] = new ItemMesh[configData.models.Length];
                    for (var i = 0; i < configData.models.Length; i++)
                    {
                        var meshData = configData.models[i];
                        if (Data.itemType == EquipItemType.Weapon && meshData.availableMeshes.Length == 2)
                        {
                            _originalItemMeshes[configData.configGuid][i] = ArmorMeshFactory.Create(MeshType.HandRight, meshLoader, meshData.prefabBundlePath, meshData.textureBundlePath);
                            _additionalItemMeshes[configData.configGuid][i] = ArmorMeshFactory.Create(MeshType.HandLeft, meshLoader, meshData.additionalPrefabBundlePath, meshData.additionalTextureBundlePath);
                            continue;
                        }

                        _originalItemMeshes[configData.configGuid][i] = ArmorMeshFactory.Create(meshData.availableMeshes[0], meshLoader, meshData.prefabBundlePath, meshData.textureBundlePath);
                        _additionalItemMeshes[configData.configGuid][i] = null;
                    }

                    _itemMeshes = new Dictionary<string, ItemMesh[]>(_originalItemMeshes);
                }
            }

            public ItemMesh[] GetItemMeshs(string configGuid, EquipItemSlot slot)
            {
                if (!_itemMeshes.ContainsKey(configGuid)) return new ItemMesh[0];
                if (Data.itemType != EquipItemType.Weapon) return _itemMeshes[configGuid];

                var meshes = new List<ItemMesh>(_itemMeshes[configGuid]);
                for (var i = 0; i < meshes.Count; i++)
                {
                    var additionalMesh = _additionalItemMeshes.ContainsKey(configGuid) ? _additionalItemMeshes[configGuid][i] : null;
                    meshes[i] = (slot == EquipItemSlot.HandLeft && additionalMesh != null)
                        ? _additionalItemMeshes[configGuid][i]
                        : _originalItemMeshes[configGuid][i];
                }
                _itemMeshes[configGuid] = meshes.ToArray();
                return _itemMeshes[configGuid];
            }

            public ItemTexture[] GetItemTextures(string configGuid)
            {
                ItemTexture[] textures;
                return _itemTextures.TryGetValue(configGuid, out textures) ? textures : new ItemTexture[0];
            }

            public void LoadTexturesAndMeshes(string guid, EquipItemSlot slotType)
            {
                var meshes = GetItemMeshs(guid, slotType);
                foreach (var itemMesh in meshes)
                    itemMesh.LoadMesh();

                ItemTexture[] textures;
                if (_itemTextures.TryGetValue(guid, out textures))
                {
                    foreach (var itemTexture in textures)
                        itemTexture.LoadTexture();
                }
            }

            public void UnloadTexturesAndMesh(string guid)
            {
                ItemMesh[] meshes;
                if (!_itemMeshes.TryGetValue(guid, out meshes)) return;

                foreach (var mesh in meshes)
                    mesh.UnLoadMesh();
            }
        }
    }
}