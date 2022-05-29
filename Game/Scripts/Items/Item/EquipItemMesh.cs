using System.Collections.Generic;
using System.Threading.Tasks;

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
                            _itemMeshes[configData.configGuid][i] = ArmorMeshFactory.Create(MeshType.HandRight, meshLoader, pathProvider.GetPath(meshData.prefab), textureLoader, pathProvider.GetPath(meshData.texture));
                            _additionalItemMeshes[configData.configGuid][i] = ArmorMeshFactory.Create(MeshType.HandLeft, meshLoader, pathProvider.GetPath(meshData.additionalPrefab), textureLoader, pathProvider.GetPath(meshData.additionalTexture));
                            continue;
                        }

                        _itemMeshes[configData.configGuid][i] = ArmorMeshFactory.Create(meshData.availableMeshes[0], meshLoader, pathProvider.GetPath(meshData.prefab), textureLoader, pathProvider.GetPath(meshData.texture));
                        _additionalItemMeshes[configData.configGuid][i] = null;
                    }
                }
            }

            public ItemMesh[] GetItemMeshes(string configGuid, bool isAdditional)
            {
                if (!_itemMeshes.TryGetValue(configGuid, out var itemMeshes)) return new ItemMesh[0];
                if (_equipItemData.itemType != EquipItemType.Weapon) return itemMeshes;

                if (!isAdditional) return itemMeshes;
                if (!_additionalItemMeshes.TryGetValue(configGuid, out var additionalItemMeshes)) return new ItemMesh[0];
                return additionalItemMeshes;
            }

            public ItemTexture[] GetItemTextures(string configGuid)
            {
                return _itemTextures.TryGetValue(configGuid, out var textures) ? textures : new ItemTexture[0];
            }

            public async Task LoadTexturesAndMeshes(string characterGuid, bool isAdditional)
            {
                var meshes = GetItemMeshes(characterGuid, isAdditional);
                var textures = GetItemTextures(characterGuid);

                var waiterTasks = new Task[meshes.Length + textures.Length];
                var i = 0;
                for (i = 0; i < meshes.Length; i++)
                    waiterTasks[i] = meshes[i].LoadMesh();

                for (var j = 0; j < textures.Length; j++)
                    waiterTasks[i + j] = textures[j].LoadTexture();

                await Task.WhenAll(waiterTasks);
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