using CharacterEditor.CharacterInventory;

public class InventoryItemDropCell : ItemDropCell
{
    protected override void OnDropInventoryItem(InventoryItemDragCell itemDrag)
    {
        GameManager.Instance.Inventory.SwapCells(_cell, itemDrag.ParentCell);
    }

    protected override void OnDropContainerItem(ContainerItemDragCell itemDrag)
    {
        if (_cell.Item != null) return;
        if (!(itemDrag.ParentCell is ContainerCell draggedContainerCeil)) return;

        GameManager.Instance.ContainerPopup.AddToInventory(draggedContainerCeil, _cell as InventoryCell);
    }


    protected override void OnDropEquippedItem(EquipItemDragCell itemDrag)
    {
        if (_cell.Item != null) return;

        var draggedItemCeil = itemDrag.ParentCell;
        if (draggedItemCeil.Item is EquipItem equipItem)
        {
            GameManager.Instance.Inventory.UnEquipItemToCell(equipItem, _cell);
            draggedItemCeil.SetItem(null);
        }
    }
}
