﻿using CharacterEditor.CharacterInventory;

public class InventoryCeil : ItemCeil
{
    protected override void OnClickHandler()
    {
        var equipItem = Item as EquipItem;
        if (equipItem != null && GameManager.Instance.Inventory.CanEquip(equipItem))
        {
            GameManager.Instance.Inventory.EquipItem(equipItem, this);
        }
    }
}
