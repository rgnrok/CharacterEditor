using UnityEngine;

public class InventoryDragCeil : ItemDragCeil
{
    protected override void DropOnGround(ItemCell itemCell, Vector3 position)
    {
        if (!(itemCell is InventoryCell inventoryCell)) return;

        GameManager.Instance.Inventory.RemoveFromInvetoryToGround(inventoryCell, position);
    }
}
