using CharacterEditor.CharacterInventory;

public class ContainerItemDropCell : ItemDropCell
{
    protected override void OnDropEquippedItem(EquipItemDragCell itemDrag)
    {
        var draggedItemCeil = itemDrag.ParentCell as EquipPanelCell;
        if (draggedItemCeil == null || _cell.Item != null) return;

        if (draggedItemCeil.Item is EquipItem)
        {
            GameManager.Instance.ContainerPopup.AddToContainer(_cell as ContainerCell, draggedItemCeil);
        }
    }

    protected override void OnDropInventoryItem(InventoryItemDragCell itemDrag)
    {
        var draggedItemCeil = itemDrag.ParentCell as InventoryCell;
        if (draggedItemCeil == null || draggedItemCeil.Item == null) return;
        if (_cell.Item != null) return;

        GameManager.Instance.ContainerPopup.AddToContainer(_cell as ContainerCell, draggedItemCeil);
    }

    protected override void OnDropContainerItem(ContainerItemDragCell itemDrag)
    {
        GameManager.Instance.ContainerPopup.SwapCeils(_cell, itemDrag.ParentCell);
    }
}
