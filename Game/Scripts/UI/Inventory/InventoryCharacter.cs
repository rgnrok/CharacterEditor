
using System.Collections.Generic;
using CharacterEditor.CharacterInventory;
using UnityEngine;
using UnityEngine.UI;

namespace CharacterEditor
{
    public class InventoryCharacter : GameUI
    {
        [SerializeField] private ScrollRect scrollView;
        [SerializeField] private GameObject ceil;

        private List<InventoryCeil> _ceils = new List<InventoryCeil>();

        public readonly Dictionary<string, Item> InventoryItems = new Dictionary<string, Item>();
        private Dictionary<int, string> _inventoryCeils = new Dictionary<int, string>();
        private Character _character;

        public string CharacterGuid { get { return _character.guid;} }

        public Dictionary<int, CharacterItemData> GetInventoryCeils()
        {
            Dictionary<int, CharacterItemData> ceils = new Dictionary<int, CharacterItemData>();

            foreach (var ceilPair in _inventoryCeils)
            {
                Item item;
                if (!InventoryItems.TryGetValue(ceilPair.Value, out item)) continue;
                ceils[ceilPair.Key] = new CharacterItemData(item);
            }
            return ceils;
        }


        public void Init(Character character)
        {
            _character = character;

            if (_inventoryCeils == null)
            {
                var characterCeils = new Dictionary<int, string>();
                foreach (var ceilPair in character.inventoryCeils)
                {
                    characterCeils[ceilPair.Key] = ceilPair.Value.guid;
                }

                _inventoryCeils = characterCeils;
            }

//            var characterItems = GetCharacterItems(character.guid);

            var maxCeilNum = 0;
            foreach (var key in _inventoryCeils.Keys)
            {
                if (key > maxCeilNum) maxCeilNum = key;
            }

            // Create new ceils
            for (var i = scrollView.content.childCount; i < maxCeilNum; i++)
            {
                Instantiate(ceil, scrollView.content.transform);
            }

            _ceils.Clear();
            for (var i = 0; i < scrollView.content.childCount; i++)
            {
                string ceilItemGuid;
                _inventoryCeils.TryGetValue(i, out ceilItemGuid);

                var itemCeil = scrollView.content.GetChild(i).GetComponent<InventoryCeil>();

                _ceils.Add(itemCeil);
                itemCeil.Index = i;

                Item item = null;
                if (ceilItemGuid != null) InventoryItems.TryGetValue(ceilItemGuid, out item);
                itemCeil.SetItem(item);
            }
        }

        public void SetItem(Item item, int ceilIndex)
        {
            InventoryItems[item.Guid] = item;
            _inventoryCeils[ceilIndex] = item.Guid;
        }

        public bool AddItem(Item item, int ceilIndex = -1)
        {
            if (TryAddItem(item, ceilIndex))
            {
                InventoryItems[item.Guid] = item;
                return true;
            }

            return false;
        }

        public void RemoveItem(string itemGuid, int ceilIndex = -1)
        {
            if (!InventoryItems.ContainsKey(itemGuid)) return;

            if (ceilIndex != -1 && _ceils.Count > ceilIndex)
            {
                SetCeilItem(_ceils[ceilIndex], null);
                return;
            }

            foreach (var pair in _inventoryCeils)
            {
                if (pair.Value != itemGuid) continue;
                SetCeilItem(_ceils[pair.Key], null);
                break;
            }

            InventoryItems.Remove(itemGuid);
        }

        private bool TryAddItem(Item item, int ceilIndex = -1)
        {
            //Not check empty ceil because unequip item drop to some ceil that eqip wil be removed
            if (ceilIndex != -1 && ceilIndex < _ceils.Count)// && _ceils[ceilIndex].IsEmpty())
            {
                SetCeilItem(_ceils[ceilIndex], item);
                return true;
            }

            foreach (var invCeil in _ceils)
            {
                if (!invCeil.IsEmpty()) continue;
                SetCeilItem(invCeil, item);
                return true;
            }

            return false;
        }

        public void SetCeilItem(ItemCeil ceil, Item item)
        {
            ceil.SetItem(item);
            if (item == null)
                _inventoryCeils.Remove(ceil.Index);
            else
                _inventoryCeils[ceil.Index] = item.Guid;
        }

        public void SwapCeils(ItemCeil ceil1, ItemCeil ceil2)
        {

            if (ceil1.Item != null)
            {
                var tmpItem = ceil2.Item;
                SetCeilItem(ceil2, ceil1.Item);
                SetCeilItem(ceil1, tmpItem);
            }
            else
            {
                SetCeilItem(ceil1, ceil2.Item);
                SetCeilItem(ceil2, null);
            }
        }


    }
}