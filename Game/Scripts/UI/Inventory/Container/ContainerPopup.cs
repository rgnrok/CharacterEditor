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

    private List<ContainerCeil> _ceils = new List<ContainerCeil>();

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

            var itemCeil = scrollView.content.GetChild(i).GetComponent<ContainerCeil>();

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

    public void AddToInventory(ContainerCeil containerCeil, InventoryCeil inventoryCeil = null)
    {
        var item = containerCeil.Item;

        if (GameManager.Instance.Inventory.AddToInvetory(item, inventoryCeil))
        {
            ContainerItems.Remove(item.Guid);
            _currentContainer.RemoveItem(containerCeil.Index);
            SetCeilItem(containerCeil, null);
        }
    }


    public void SwapCeils(ItemCeil ceil1, ItemCeil ceil2)
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

    private void SetCeilItem(ItemCeil ceil, Item item)
    {
        ceil.SetItem(item);
        if (item == null)
            _containerCeils.Remove(ceil.Index);
        else
            _containerCeils[ceil.Index] = new CharacterItemData(item);
    }

    public void AddToContainer(ContainerCeil containerCeil, InventoryCeil inventoryCeil)
    {
        var item = inventoryCeil.Item;
        if (item == null) return;

        ContainerItems[item.Guid] = item;
        GameManager.Instance.Inventory.RemoveFromInvetory(inventoryCeil);
        _currentContainer.AddItem(containerCeil.Index, item);
        SetCeilItem(containerCeil, item);
    }


    public void AddToContainer(ContainerCeil containerCeil, EquipPanelCeil equipCeil)
    {
        var item = equipCeil.Item as EquipItem;
        if (item == null) return;

        ItemManager.Instance.UnEquipItem(item);
        equipCeil.SetItem(null);

        ContainerItems[item.Guid] = item;
        _currentContainer.AddItem(containerCeil.Index, item);
        SetCeilItem(containerCeil, item);
    }
}
