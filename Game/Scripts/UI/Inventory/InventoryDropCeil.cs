using CharacterEditor.CharacterInventory;

public class InventoryDropCeil : DropCeil {

    protected override void OnDropItem(ItemDragCeil drag)
    {
        GameManager.Instance.Inventory.SwapCeils(_ceil, drag.ParentCeil);
    }

    protected override void OnDropInventoryItem(InventoryDragCeil drag)
    {
        GameManager.Instance.Inventory.SwapCeils(_ceil, drag.ParentCeil);
    }

    protected override void OnDropContainerItem(ContainerDragCeil drag)
    {
        var draggedContainerCeil = drag.ParentCeil as ContainerCeil;
        if (_ceil.Item != null) return;

        GameManager.Instance.ContainerPopup.AddToInventory(draggedContainerCeil, _ceil as InventoryCeil);
    }


    protected override void OnDropEquippedItem(EquipDragCeil drag)
    {
        var draggedItemCeil = drag.ParentCeil;
        if (_ceil.Item != null) return;

        var equipItem = draggedItemCeil.Item as EquipItem;
        if (equipItem != null)
        {
            GameManager.Instance.Inventory.UnEquipItemToCeil(equipItem, _ceil);
            draggedItemCeil.SetItem(null);
        }

    }
}
