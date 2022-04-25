using System;
using CharacterEditor.CharacterInventory;

public class EquipDropCeil : DropCeil {

    protected EquipItemType[] _itemTypes;
    private EquipItemSlot _slot;


    public void Start()
    {
        var ceil = GetComponent<EquipPanelCeil>();
        if (ceil != null)
        {
            _slot = ceil.ItemSlot;
            _itemTypes = ceil.AvailableTypes;
        }
    }

    protected override void OnDropItem(ItemDragCeil drag)
    {
        
    }
    protected override void OnDropContainerItem(ContainerDragCeil drag)
    {
        
    }

    protected override void OnDropInventoryItem(InventoryDragCeil drag)
    {
        var item = drag.ParentCeil.Item as EquipItem;
        if (item == null || _itemTypes == null || Array.IndexOf(_itemTypes, item.ItemType) == -1) return;
        if (!ItemManager.Instance.CanEquip(item)) return;

        // todo maybe need force unequip old item
        ItemManager.Instance.EquipItem(item, _slot);
    }

    protected override void OnDropEquippedItem(EquipDragCeil drag)
    {
        var parentCeil = drag.ParentCeil as EquipPanelCeil;
        if (parentCeil == null) return;

        var newItem = parentCeil.Item as EquipItem;
        var currentItem = _ceil.Item as EquipItem;
        if (newItem == null || Array.IndexOf(_itemTypes, newItem.ItemType) == -1) return;
        if (_ceil.Item != null && Array.IndexOf(parentCeil.AvailableTypes, currentItem.ItemType) == -1) return;
        
        ItemManager.Instance.SwapEquipedItem(_slot, parentCeil.ItemSlot);
    }
}
