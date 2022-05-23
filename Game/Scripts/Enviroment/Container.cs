using System.Collections;
using System.Collections.Generic;
using CharacterEditor.CharacterInventory;
using CharacterEditor;
using CharacterEditor.Services;
using UnityEngine;

namespace CharacterEditor
{
    public class Container : MonoBehaviour
    {
        public string Guid { get; private set; }
        private Dictionary<int, Item> _items = new Dictionary<int, Item>();
        private ITextureLoader _textureLoader;
        private IMeshLoader _meshLoader;
        private IPathDataProvider _pathProvider;

        private void Awake()
        {
            var loadService = AllServices.Container.Single<ILoaderService>();
            _textureLoader = loadService.TextureLoader;
            _meshLoader = loadService.MeshLoader;
            _pathProvider = loadService.PathDataProvider;
        }

        public void SetData(ContainerSaveData containerData, Dictionary<int, ItemData> items)
        {
            Guid = containerData.guid;
            foreach (var itemPair in items)
            {
                var equipItemData = itemPair.Value as EquipItemData;
                if (equipItemData != null)
                {
                    var stats = equipItemData.stats;
                    if (containerData.items.TryGetValue(itemPair.Key, out var characterItem) && characterItem.guid == itemPair.Value.guid)
                        stats = characterItem.stats;

                    var eiMesh = new EquipItemMesh(equipItemData, _textureLoader, _meshLoader, _pathProvider);
                    _items.Add(itemPair.Key, new EquipItem(null, equipItemData, eiMesh, stats)); //todo guid null
                }
                else
                {
                    _items.Add(itemPair.Key, new Item(itemPair.Value));
                }
            }
        }

        public void AddItem(int ceilIndex, Item item)
        {
            if (item == null)
            {
                RemoveItem(ceilIndex);
                return;
            }
            _items.Add(ceilIndex, item);
        }

        public void RemoveItem(int index)
        {
            _items.Remove(index);
        }

        public Dictionary<int, Item> GetItems()
        {
            return _items;
        }
    }
}