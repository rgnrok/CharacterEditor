using System;
using System.Collections;
using System.Collections.Generic;
using CharacterEditor.CharacterInventory;
using UnityEngine;

public class ContainerDropCeil : DropCeil
{
    protected override void OnDropItem(ItemDragCeil drag)
    {
        GameManager.Instance.ContainerPopup.SwapCeils(Cell, drag.ParentCell);

    }

    protected override void OnDropEquippedItem(EquipDragCeil drag)
    {
        var draggedItemCeil = drag.ParentCell as EquipPanelCell;
        if (Cell.Item != null) return;

        var equipItem = draggedItemCeil.Item as EquipItem;
        if (equipItem != null)
        {
            GameManager.Instance.ContainerPopup.AddToContainer(Cell as ContainerCell, draggedItemCeil);
        }
    }

    protected override void OnDropInventoryItem(InventoryDragCeil drag)
    {
        var draggedItemCeil = drag.ParentCell as InventoryCell;
        if (draggedItemCeil == null || draggedItemCeil.Item == null) return;
        if (Cell.Item != null) return;

        GameManager.Instance.ContainerPopup.AddToContainer(Cell as ContainerCell, draggedItemCeil);
    }

    protected override void OnDropContainerItem(ContainerDragCeil drag)
    {
        GameManager.Instance.ContainerPopup.SwapCeils(Cell, drag.ParentCell);
    }
}
