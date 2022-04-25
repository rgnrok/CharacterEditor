using System;
using System.Collections;
using System.Collections.Generic;
using CharacterEditor.CharacterInventory;
using UnityEngine;

public class ContainerDropCeil : DropCeil
{
    protected override void OnDropItem(ItemDragCeil drag)
    {
        GameManager.Instance.ContainerPopup.SwapCeils(_ceil, drag.ParentCeil);

    }

    protected override void OnDropEquippedItem(EquipDragCeil drag)
    {
        var draggedItemCeil = drag.ParentCeil as EquipPanelCeil;
        if (_ceil.Item != null) return;

        var equipItem = draggedItemCeil.Item as EquipItem;
        if (equipItem != null)
        {
            GameManager.Instance.ContainerPopup.AddToContainer(_ceil as ContainerCeil, draggedItemCeil);
        }
    }

    protected override void OnDropInventoryItem(InventoryDragCeil drag)
    {
        var draggedItemCeil = drag.ParentCeil as InventoryCeil;
        if (draggedItemCeil == null || draggedItemCeil.Item == null) return;
        if (_ceil.Item != null) return;

        GameManager.Instance.ContainerPopup.AddToContainer(_ceil as ContainerCeil, draggedItemCeil);
    }

    protected override void OnDropContainerItem(ContainerDragCeil drag)
    {
        GameManager.Instance.ContainerPopup.SwapCeils(_ceil, drag.ParentCeil);
    }
}
