using UnityEngine;

public class InventoryDragCeil : ItemDragCeil
{
    protected override void DropOnGround(ItemCeil itemCeil, Vector3 position)
    {
        var inventoryCeil = itemCeil as InventoryCeil;
        if (inventoryCeil == null) return;

        GameManager.Instance.Inventory.RemoveFromInvetoryToGround(inventoryCeil, position);
    }
}
