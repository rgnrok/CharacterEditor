using CharacterEditor.CharacterInventory;

public class InventoryDropCeil : DropCeil {

    protected override void OnDropItem(ItemDragCeil drag)
    {
        GameManager.Instance.Inventory.SwapCells(Cell, drag.ParentCell);
    }

    protected override void OnDropInventoryItem(InventoryDragCeil drag)
    {
        GameManager.Instance.Inventory.SwapCells(Cell, drag.ParentCell);
    }

    protected override void OnDropContainerItem(ContainerDragCeil drag)
    {
        if (Cell.Item != null) return;
        if (!(drag.ParentCell is ContainerCell draggedContainerCeil)) return;

        GameManager.Instance.ContainerPopup.AddToInventory(draggedContainerCeil, Cell as InventoryCell);
    }


    protected override void OnDropEquippedItem(EquipDragCeil drag)
    {
        if (Cell.Item != null) return;

        var draggedItemCeil = drag.ParentCell;
        if (draggedItemCeil.Item is EquipItem equipItem)
        {
            GameManager.Instance.Inventory.UnEquipItemToCell(equipItem, Cell);
            draggedItemCeil.SetItem(null);
        }
    }
}
