using System;
using CharacterEditor;
using CharacterEditor.CharacterInventory;
using CharacterEditor.Services;

public class EquipItemDropCell : ItemDropCell {

    protected EquipItemType[] _itemTypes;
    private EquipItemSlot _slot;
    private ICharacterEquipItemService _characterEquipItemService;

    protected override void Awake()
    {
        base.Awake();
        _characterEquipItemService = AllServices.Container.Single<ICharacterEquipItemService>();
    }

    public void Start()
    {
        var ceil = GetComponent<EquipPanelCell>();
        if (ceil != null)
        {
            _slot = ceil.ItemSlot;
            _itemTypes = ceil.AvailableTypes;
        }
    }

    protected override void OnDropContainerItem(ContainerItemDragCell itemDrag)
    {
        
    }

    protected override void OnDropInventoryItem(InventoryItemDragCell itemDrag)
    {
        var item = itemDrag.ParentCell.Item as EquipItem;
        if (item == null || _itemTypes == null || Array.IndexOf(_itemTypes, item.ItemType) == -1) return;
        if (!_characterEquipItemService.CanEquip(item)) return;

        // todo maybe need force unequip old item
        _characterEquipItemService.EquipItem(item, _slot);
    }

    protected override void OnDropEquippedItem(EquipItemDragCell itemDrag)
    {
        var oldCell = itemDrag.ParentCell as EquipPanelCell;
        if (oldCell == null) return;

        var newItem = oldCell.Item as EquipItem;
        var currentItem = _cell.Item as EquipItem;
        if (newItem == null || Array.IndexOf(_itemTypes, newItem.ItemType) == -1) return;
        if (currentItem == null || Array.IndexOf(oldCell.AvailableTypes, currentItem.ItemType) == -1) return;

        _characterEquipItemService.SwapEquippedItem(_slot, oldCell.ItemSlot);

        oldCell.SetItem(currentItem);
        _cell.SetItem(newItem);
    }
}
