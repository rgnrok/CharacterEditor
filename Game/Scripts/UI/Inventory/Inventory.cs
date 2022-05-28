using System.Collections;
using System.Collections.Generic;
using CharacterEditor;
using CharacterEditor.CharacterInventory;
using CharacterEditor.Services;
using UnityEngine;
using UnityEngine.UI;

public class Inventory : Popup
{

//    [SerializeField] private GameObject cell;
    [SerializeField] private GameObject inventoryCharacterItemPrefab;
//    [SerializeField] private ScrollRect scrollView;

//    private List<InventoryCell> _cells = new List<InventoryCell>();
    private Dictionary<string, InventoryCharacter> _inventoryCharacters = new Dictionary<string, InventoryCharacter>();
    private Character _currentCharacter;

    //    private Dictionary<string, Dictionary<int, CharacterItemData>> _inventoryCells = new Dictionary<string, Dictionary<int, CharacterItemData>>();
    //    public readonly Dictionary<string, Item> InventoryItems = new Dictionary<string, Item>();

//    public readonly Dictionary<string, Dictionary<string, Item>> InventoryItems = new Dictionary<string, Dictionary<string, Item>>();
//    private readonly Dictionary<string, Dictionary<int, string>> _inventoryCells = new Dictionary<string, Dictionary<int, string>>();

    private readonly Dictionary<string, int> _waitingAddItemCells = new Dictionary<string, int>();
    private readonly Dictionary<string, int> _waitingRemoveItemCells = new Dictionary<string, int>();
    private ICommonLoader<GameObject> _gameObjectLoader;
    private IPathDataProvider _pathProvider;


    protected override void OnEnable()
    {
        Init(GameManager.Instance.CurrentCharacter); //todo
        _gameObjectLoader = AllServices.Container.Single<ILoaderService>().GameObjectLoader;
        _pathProvider = AllServices.Container.Single<ILoaderService>().PathDataProvider;

        base.OnEnable();
    }

    public Dictionary<int, CharacterItemData> GetInventoryCells(string characterId)
    {
        if (_inventoryCharacters.TryGetValue(characterId, out var characteInv))
            return characteInv.GetInventoryCells();

        return null;
    }

    public Dictionary<string, Item> GetCharacterItems(string characterId)
    {
        if (_inventoryCharacters.TryGetValue(characterId, out var characteInv))
            return characteInv.InventoryItems;

        return null;
    }

    public void Init(Character character)
    {
        if (_currentCharacter != null)
        {
            _currentCharacter.OnAddToInventory -= AddToInventoryHandler;
            _currentCharacter.OnRemoveFromInventory -= RemoveFromInventoryHandler;
        }
        _currentCharacter = character;

        // Add new
        foreach (var characterInfo in GameManager.Instance.Characters.Values)
        {
            if (!_inventoryCharacters.ContainsKey(characterInfo.Guid))
            {
                var item = AddChild(inventoryCharacterItemPrefab).GetComponent<InventoryCharacter>();
                _inventoryCharacters[characterInfo.Guid] = item;
            }
            _inventoryCharacters[characterInfo.Guid].Init(characterInfo);
        }

        //Remove old
        var keys = _inventoryCharacters.Keys;
        foreach (var characterInventoryGuid in keys)
        {
            if (!GameManager.Instance.Characters.ContainsKey(characterInventoryGuid))
            {
                Destroy(_inventoryCharacters[characterInventoryGuid].gameObject);
                _inventoryCharacters.Remove(characterInventoryGuid);
            }
        }

        _currentCharacter.OnAddToInventory += AddToInventoryHandler;
        _currentCharacter.OnRemoveFromInventory += RemoveFromInventoryHandler;
        InnerUpdate();
    }




    public bool AddToInventory(Item item)
    {
        if (_currentCharacter == null) return false;

        if (!_inventoryCharacters.TryGetValue(_currentCharacter.Guid, out var inventoryCharacter)) return false;
        return inventoryCharacter.AddItem(item);
    }

    public bool AddToInventory(PickUpItem pickUpItem)
    {
        var item = pickUpItem.Item;
        if (item == null) return false;

        if (!AddToInventory(item)) return false;
        Destroy(pickUpItem.gameObject);
        return true;

    }

    public bool AddToInventory(Item item, InventoryCell cell)
    {
        if (cell == null)
        {
            return AddToInventory(item);
        }

        var cell1Character = cell.GetComponentInParent<InventoryCharacter>();
        return cell1Character.AddItem(item, cell.Index);
    }

    public void SetItemToInvetory(string characterGuid, Item item, int cellIndex)
    {
        if (!_inventoryCharacters.ContainsKey(characterGuid))
            _inventoryCharacters[characterGuid] = AddChild(inventoryCharacterItemPrefab).GetComponent<InventoryCharacter>(); ;

        _inventoryCharacters[characterGuid].SetItem(item, cellIndex);
    }

 

    public void RemoveFromInvetory(InventoryCell cell)
    {
        var item = cell.Item;
        if (item == null) return;

        var cell1Character = cell.GetComponentInParent<InventoryCharacter>();
        cell1Character.RemoveItem(item.Guid, cell.Index);
    }

    public void RemoveFromInvetoryToGround(InventoryCell cell, Vector3 groundPosition)
    {
        var item = cell.Item;
        RemoveFromInvetory(cell);
        //todo
        _gameObjectLoader.LoadByPath(_pathProvider.GetPath(item.Data.prefab), (path, prefab) =>
            {
                var instance = Instantiate(prefab);
                instance.gameObject.layer = Constants.LAYER_PICKUP;
                var pickupItem = instance.GetComponent<PickUpItem>();
                if (pickupItem != null)
                {
                    pickupItem.Item = item;
                }

                GameManager.Instance.StartCoroutine(DropItemCoroutine(instance,
                    _currentCharacter.GameObjectData.Entity.transform.position, groundPosition));
            });
    }

    private IEnumerator DropItemCoroutine(GameObject prefab, Vector3 from, Vector3 to)
    {
        //todo
        for (int i = 0; i < 20; i++)
        {
            prefab.transform.position = Vector3.LerpUnclamped(from, to, i / 20f);
            yield return null;
        }
    }
    



    private void AddToInventoryHandler(string characterId, Item item)
    {
        if (_waitingAddItemCells.Count != 0)
        {
            foreach (var pair in _waitingAddItemCells)
            {
                _inventoryCharacters[pair.Key].AddItem(item, pair.Value);
                break;
            }
            _waitingAddItemCells.Clear();
            return;
        }

        _inventoryCharacters[characterId].AddItem(item);
    }


    private void RemoveFromInventoryHandler(string characterId, Item item)
    {
        _waitingAddItemCells.Clear();

        if (_waitingRemoveItemCells.Count != 0)
        {
            foreach (var pair in _waitingRemoveItemCells)
            {
                _inventoryCharacters[pair.Key].RemoveItem(item.Guid);
                break;
            }
            _waitingRemoveItemCells.Clear();
            return;
        }

        _inventoryCharacters[characterId].RemoveItem(item.Guid);
    }

    public void SwapCells(ItemCell cell1, ItemCell cell2)
    {
        var cell1Character = cell1.GetComponentInParent<InventoryCharacter>();
        var cell2Character = cell2.GetComponentInParent<InventoryCharacter>();

        if (cell1Character.CharacterGuid == cell2Character.CharacterGuid)
        {
            cell1Character.SwapCells(cell1, cell2);
            return;
        }

        var cell1Item = cell1.Item;
        var cell2Item = cell2.Item;
        if (cell1Item != null) cell1Character.RemoveItem(cell1Item.Guid);
        if (cell2Item != null) cell2Character.RemoveItem(cell2Item.Guid);

        if (cell2Item != null) cell1Character.AddItem(cell2Item, cell1.Index);
        if (cell1Item != null) cell2Character.AddItem(cell1Item, cell2.Index);
    }


    //todo work only with current character
    public void UnEquipItemToCell(EquipItem equipItem, ItemCell cell)
    {
        var cellCharacter = cell.GetComponentInParent<InventoryCharacter>();
        _waitingAddItemCells[cellCharacter.CharacterGuid] = cell.Index;

        ItemManager.Instance.UnEquipItem(equipItem);
        AddToInventory(equipItem);
    }


    public bool CanEquip(EquipItem item)
    {
        return true;
    }


    public void EquipItem(EquipItem equipItem, ItemCell cell)
    {
        var cellCharacter = cell.GetComponentInParent<InventoryCharacter>();
        _waitingRemoveItemCells[cellCharacter.CharacterGuid] = cell.Index;
        _waitingAddItemCells[cellCharacter.CharacterGuid] = cell.Index;

        ItemManager.Instance.EquipItem(equipItem);
    }
}
