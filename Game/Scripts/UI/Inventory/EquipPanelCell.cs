using System.Collections.Generic;
using CharacterEditor.CharacterInventory;
using UnityEngine;

public class EquipPanelCell : ItemCell
{
    [SerializeField] private EquipItemSlot _slot;
    [SerializeField] private EquipItemType[] _availableItemTypes;

    public EquipItemSlot ItemSlot { get { return _slot; } }
    public EquipItemType[] AvailableTypes { get { return _availableItemTypes; } }
    
    protected override void OnClickHandler()
    {
        var equipItem = Item as EquipItem;
        if (equipItem != null)
        {
            ItemManager.Instance.UnEquipItem(equipItem);
            GameManager.Instance.Inventory.AddToInventory(equipItem);
            SetItem(null);
        }
    }
}