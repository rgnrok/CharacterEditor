using System.Collections.Generic;

namespace CharacterEditor
{
    namespace CharacterInventory
    {
        public class EquipItemMesh
        {
            private readonly Dictionary<string, ItemTexture[]> _itemTextures = new Dictionary<string, ItemTexture[]>();

            private readonly Dictionary<string, ItemMesh[]> _itemMeshes = new Dictionary<string, ItemMesh[]>();
            private readonly Dictionary<string, ItemMesh[]> _additionalItemMeshes = new Dictionary<string, ItemMesh[]>();

            private readonly EquipItemData _equipItemData;

            public bool IsReady(string configGuid)
            {
                if (_itemTextures.TryGetValue(configGuid, out var textures))
                {
                    foreach (var texture in textures)
                        if (!texture.IsReady) return false;
                }

                if (_itemMeshes.TryGetValue(configGuid, out var meshes))
                {
                    foreach (var mesh in meshes)
                        if (!mesh.IsReady) return false;
                }

                return true;
            }

            public EquipItemMesh(EquipItemData itemData, ITextureLoader textureLoader, IMeshLoader meshLoader, IPathDataProvider pathProvider)
            {
                _equipItemData = itemData;

                foreach (var configData in _equipItemData.configsItems)
                {
                    _itemTextures[configData.configGuid] = new ItemTexture[configData.textures.Length];
                    for (var i = 0; i < configData.textures.Length; i++)
                    {
                        var textureData = configData.textures[i];
                        _itemTextures[configData.configGuid][i] = ClothTextureFactory.Create(textureData.textureType, textureLoader, pathProvider.GetPath(textureData.texture));
                    }

                    var meshesCount = configData.models.Length;
                    _itemMeshes[configData.configGuid] = new ItemMesh[meshesCount];
                    _additionalItemMeshes[configData.configGuid] = new ItemMesh[meshesCount];
                    for (var i = 0; i < meshesCount; i++)
                    {
                        var meshData = configData.models[i];
                        if (_equipItemData.itemType == EquipItemType.Weapon && meshData.availableMeshes.Length == 2)
                        {
                            _itemMeshes[configData.configGuid][i] = ArmorMeshFactory.Create(MeshType.HandRight, meshLoader, pathProvider.GetPath(meshData.prefab), pathProvider.GetPath(meshData.texture));
                            _additionalItemMeshes[configData.configGuid][i] = ArmorMeshFactory.Create(MeshType.HandLeft, meshLoader, pathProvider.GetPath(meshData.additionalPrefab), pathProvider.GetPath(meshData.additionalTexture));
                            continue;
                        }

                        _itemMeshes[configData.configGuid][i] = ArmorMeshFactory.Create(meshData.availableMeshes[0], meshLoader, pathProvider.GetPath(meshData.prefab), pathProvider.GetPath(meshData.texture));
                        _additionalItemMeshes[configData.configGuid][i] = null;
                    }
                }
            }

            public IEnumerable<ItemMesh> GetItemMeshes(string configGuid, bool isAdditional)
            {
                if (!_itemMeshes.TryGetValue(configGuid, out var itemMeshes)) return new ItemMesh[0];
                if (_equipItemData.itemType != EquipItemType.Weapon) return itemMeshes;

                if (!isAdditional) return itemMeshes;
                if (!_additionalItemMeshes.TryGetValue(configGuid, out var additionalItemMeshes)) return new ItemMesh[0];
                return additionalItemMeshes;
            }

            public IEnumerable<ItemTexture> GetItemTextures(string configGuid)
            {
                return _itemTextures.TryGetValue(configGuid, out var textures) ? textures : new ItemTexture[0];
            }

            public void LoadTexturesAndMeshes(string characterGuid, bool isAdditional)
            {
                var meshes = GetItemMeshes(characterGuid, isAdditional);
                foreach (var itemMesh in meshes)
                    itemMesh.LoadMesh();

                var textures = GetItemTextures(characterGuid);
                foreach (var itemTexture in textures)
                    itemTexture.LoadTexture();
            }

            public void UnloadTexturesAndMesh(string characterGuid)
            {
                if (!_itemMeshes.TryGetValue(characterGuid, out var meshes)) return;

                foreach (var mesh in meshes)
                    mesh.UnLoadMesh();
            }
        }
    }
}