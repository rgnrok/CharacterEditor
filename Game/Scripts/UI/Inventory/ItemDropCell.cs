using UnityEngine;
using UnityEngine.EventSystems;

public abstract class ItemDropCell : MonoBehaviour, IDropHandler
{
    protected ItemCell _cell;

    protected virtual void Awake()
    {
        _cell = GetComponent<ItemCell>();
    }

    public virtual void OnDrop(PointerEventData eventData)
    {
        if (_cell == null) return;

        var drag = eventData.pointerDrag.GetComponent<ItemDragCeil>();
        if (drag == null) return;

        DropItem(drag);
        drag.OnEndDragHandler();
    }


    private void DropItem(ItemDragCeil drag)
    {
        var equipDragCeil = drag as EquipItemDragCell;
        if (equipDragCeil != null)
        {
            OnDropEquippedItem(equipDragCeil);
            return;
        }

        var inventoryDragCeil = drag as InventoryItemDragCell;
        if (inventoryDragCeil != null)
        {
            OnDropInventoryItem(inventoryDragCeil);
            return;
        }

        var containerDragCeil = drag as ContainerItemDragCell;
        if (containerDragCeil != null)
        {
            OnDropContainerItem(containerDragCeil);
            return;
        }
    }

    protected abstract void OnDropEquippedItem(EquipItemDragCell itemDrag);
    protected abstract void OnDropInventoryItem(InventoryItemDragCell itemDrag);
    protected abstract void OnDropContainerItem(ContainerItemDragCell itemDrag);
}
