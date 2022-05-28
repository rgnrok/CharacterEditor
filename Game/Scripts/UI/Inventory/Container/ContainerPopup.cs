using System.Collections;
using System.Collections.Generic;
using CharacterEditor;
using CharacterEditor.CharacterInventory;
using UnityEngine;
using UnityEngine.UI;

public class ContainerPopup : Popup
{

    [SerializeField] private GameObject ceil;
    [SerializeField] private ScrollRect scrollView;

    private List<ContainerCell> _ceils = new List<ContainerCell>();

    private Dictionary<int, CharacterItemData> _containerCeils = new Dictionary<int, CharacterItemData>();
    public readonly Dictionary<string, Item> ContainerItems = new Dictionary<string, Item>();

    private Character _currentCharacter;
    private Container _currentContainer;




    public void Init(Container container)
    {
        _currentContainer = container;
        var items = container.GetItems();

        _ceils.Clear();
        if (_currentCharacter != null)
        {
//            _currentCharacter.OnAddToInventory -= AddToInventoryHandler;
        }
        _currentCharacter = GameManager.Instance.CurrentCharacter;

        //todo
        _containerCeils.Clear();
        ContainerItems.Clear();
        foreach (var itemPair in items)
        {
            var item = itemPair.Value;
            _containerCeils[itemPair.Key] = new CharacterItemData(item);
            ContainerItems[item.Guid] = item;
        }
        //end todo

        var maxCeilNum = 0;
        foreach (var key in _containerCeils.Keys)
        {
            if (key > maxCeilNum) maxCeilNum = key;
        }


        // Create new ceils
        for (var i = scrollView.content.childCount; i < maxCeilNum; i++)
        {
            Instantiate(ceil, scrollView.content.transform);
        }

        for (var i = 0; i < scrollView.content.childCount; i++)
        {
            CharacterItemData ceilInfo;
            _containerCeils.TryGetValue(i, out ceilInfo);

            var itemCeil = scrollView.content.GetChild(i).GetComponent<ContainerCell>();

            _ceils.Add(itemCeil);
            itemCeil.Index = i;

            Item item = null;
            if (ceilInfo != null) ContainerItems.TryGetValue(ceilInfo.guid, out item);
            itemCeil.SetItem(item);
        }

//        _currentCharacter.OnAddToInventory += AddToInventoryHandler;
    }


    public void TakeAll()
    {
        foreach (var ceil in _ceils)
        {
            if (ceil.IsEmpty()) continue;
            AddToInventory(ceil);
        }
    }

    public void AddToInventory(ContainerCell containerCell, InventoryCell inventoryCell = null)
    {
        var item = containerCell.Item;

        if (GameManager.Instance.Inventory.AddToInventory(item, inventoryCell))
        {
            ContainerItems.Remove(item.Guid);
            _currentContainer.RemoveItem(containerCell.Index);
            SetCeilItem(containerCell, null);
        }
    }


    public void SwapCeils(ItemCell ceil1, ItemCell ceil2)
    {
        if (ceil1.Item != null)
        {
            var tmpItem = ceil2.Item;
            SetCeilItem(ceil2, ceil1.Item);
            SetCeilItem(ceil1, tmpItem);
        }
        else
        {
            SetCeilItem(ceil1, ceil2.Item);
            SetCeilItem(ceil2, null);
        }

        _currentContainer.RemoveItem(ceil1.Index);
        _currentContainer.RemoveItem(ceil2.Index);
        _currentContainer.AddItem(ceil1.Index, ceil1.Item);
        _currentContainer.AddItem(ceil2.Index, ceil2.Item);
    }

    private void SetCeilItem(ItemCell cell, Item item)
    {
        cell.SetItem(item);
        if (item == null)
            _containerCeils.Remove(cell.Index);
        else
            _containerCeils[cell.Index] = new CharacterItemData(item);
    }

    public void AddToContainer(ContainerCell containerCell, InventoryCell inventoryCell)
    {
        var item = inventoryCell.Item;
        if (item == null) return;

        ContainerItems[item.Guid] = item;
        GameManager.Instance.Inventory.RemoveFromInvetory(inventoryCell);
        _currentContainer.AddItem(containerCell.Index, item);
        SetCeilItem(containerCell, item);
    }


    public void AddToContainer(ContainerCell containerCell, EquipPanelCell equipCell)
    {
        var item = equipCell.Item as EquipItem;
        if (item == null) return;

        ItemManager.Instance.UnEquipItem(item);
        equipCell.SetItem(null);

        ContainerItems[item.Guid] = item;
        _currentContainer.AddItem(containerCell.Index, item);
        SetCeilItem(containerCell, item);
    }
}
