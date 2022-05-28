using UnityEngine;
using UnityEngine.EventSystems;

public abstract class DropCeil : MonoBehaviour, IDropHandler
{
    protected ItemCell Cell;

    void Awake()
    {
        Cell = GetComponent<ItemCell>();
    }

    public virtual void OnDrop(PointerEventData eventData)
    {
        var drag = eventData.pointerDrag.GetComponent<ItemDragCeil>();
        if (drag == null || Cell == null) return;


        DropItem(drag);
        drag.OnEndDragHandler();
    }

    protected void DropItem(ItemDragCeil drag)
    {
        var equipDragCeil = drag as EquipDragCeil;
        if (equipDragCeil != null)
        {
            OnDropEquippedItem(equipDragCeil);
            return;
        }

        var inventoryDragCeil = drag as InventoryDragCeil;
        if (inventoryDragCeil != null)
        {
            OnDropInventoryItem(inventoryDragCeil);
            return;
        }

        var containerDragCeil = drag as ContainerDragCeil;
        if (containerDragCeil != null)
        {
            OnDropContainerItem(containerDragCeil);
            return;
        }

        OnDropItem(drag);
    }

    protected abstract void OnDropItem(ItemDragCeil drag);
    protected abstract void OnDropEquippedItem(EquipDragCeil drag);
    protected abstract void OnDropInventoryItem(InventoryDragCeil drag);
    protected abstract void OnDropContainerItem(ContainerDragCeil drag);
}
