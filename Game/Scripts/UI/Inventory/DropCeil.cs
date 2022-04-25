using UnityEngine;
using UnityEngine.EventSystems;

public abstract class DropCeil : MonoBehaviour, IDropHandler
{
    protected ItemCeil _ceil;

    void Awake()
    {
        _ceil = GetComponent<ItemCeil>();
    }

    public virtual void OnDrop(PointerEventData eventData)
    {
        var drag = eventData.pointerDrag.GetComponent<ItemDragCeil>();
        if (drag == null || _ceil == null) return;


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
