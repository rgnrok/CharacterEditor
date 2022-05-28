
using System.Collections.Generic;
using CharacterEditor.CharacterInventory;
using UnityEngine;
using UnityEngine.UI;

namespace CharacterEditor
{
    public class InventoryCharacter : GameUI
    {
        [SerializeField] private ScrollRect scrollView;
        [SerializeField] private GameObject cell;

        private List<InventoryCell> _cells = new List<InventoryCell>();

        public readonly Dictionary<string, Item> InventoryItems = new Dictionary<string, Item>();
        private Dictionary<int, string> _inventoryCells = new Dictionary<int, string>();
        private Character _character;

        public string CharacterGuid { get { return _character.Guid;} }

        public Dictionary<int, CharacterItemData> GetInventoryCells()
        {
            Dictionary<int, CharacterItemData> cells = new Dictionary<int, CharacterItemData>();

            foreach (var cellPair in _inventoryCells)
            {
                if (!InventoryItems.TryGetValue(cellPair.Value, out var item)) continue;
                cells[cellPair.Key] = new CharacterItemData(item);
            }
            return cells;
        }


        public void Init(Character character)
        {
            _character = character;

            if (_inventoryCells == null)
            {
                var characterCells = new Dictionary<int, string>();
                foreach (var cellPair in character.InventoryCells)
                {
                    characterCells[cellPair.Key] = cellPair.Value.guid;
                }

                _inventoryCells = characterCells;
            }

//            var characterItems = GetCharacterItems(character.guid);

            var maxCellNum = 0;
            foreach (var key in _inventoryCells.Keys)
            {
                if (key > maxCellNum) maxCellNum = key;
            }

            // Create new cells
            for (var i = scrollView.content.childCount; i < maxCellNum; i++)
            {
                Instantiate(cell, scrollView.content.transform);
            }

            _cells.Clear();
            for (var i = 0; i < scrollView.content.childCount; i++)
            {
                string cellItemGuid;
                _inventoryCells.TryGetValue(i, out cellItemGuid);

                var itemCell = scrollView.content.GetChild(i).GetComponent<InventoryCell>();

                _cells.Add(itemCell);
                itemCell.Index = i;

                Item item = null;
                if (cellItemGuid != null) InventoryItems.TryGetValue(cellItemGuid, out item);
                itemCell.SetItem(item);
            }
        }

        public void SetItem(Item item, int cellIndex)
        {
            InventoryItems[item.Guid] = item;
            _inventoryCells[cellIndex] = item.Guid;
        }

        public bool AddItem(Item item, int cellIndex = -1)
        {
            if (TryAddItem(item, cellIndex))
            {
                InventoryItems[item.Guid] = item;
                return true;
            }

            return false;
        }

        public void RemoveItem(string itemGuid, int cellIndex = -1)
        {
            if (!InventoryItems.ContainsKey(itemGuid)) return;

            if (cellIndex != -1 && _cells.Count > cellIndex)
            {
                SetCellItem(_cells[cellIndex], null);
                return;
            }

            foreach (var pair in _inventoryCells)
            {
                if (pair.Value != itemGuid) continue;
                SetCellItem(_cells[pair.Key], null);
                break;
            }

            InventoryItems.Remove(itemGuid);
        }

        private bool TryAddItem(Item item, int cellIndex = -1)
        {
            //Not check empty cell because unequip item drop to some cell that eqip wil be removed
            if (cellIndex != -1 && cellIndex < _cells.Count)// && _cells[cellIndex].IsEmpty())
            {
                SetCellItem(_cells[cellIndex], item);
                return true;
            }

            foreach (var invCell in _cells)
            {
                if (!invCell.IsEmpty()) continue;
                SetCellItem(invCell, item);
                return true;
            }

            return false;
        }

        public void SetCellItem(ItemCell cell, Item item)
        {
            cell.SetItem(item);
            if (item == null)
                _inventoryCells.Remove(cell.Index);
            else
                _inventoryCells[cell.Index] = item.Guid;
        }

        public void SwapCells(ItemCell cell1, ItemCell cell2)
        {

            if (cell1.Item != null)
            {
                var tmpItem = cell2.Item;
                SetCellItem(cell2, cell1.Item);
                SetCellItem(cell1, tmpItem);
            }
            else
            {
                SetCellItem(cell1, cell2.Item);
                SetCellItem(cell2, null);
            }
        }


    }
}