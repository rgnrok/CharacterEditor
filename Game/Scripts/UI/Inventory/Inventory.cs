using System.Collections;
using System.Collections.Generic;
using CharacterEditor;
using CharacterEditor.CharacterInventory;
using CharacterEditor.Services;
using UnityEngine;
using UnityEngine.UI;

public class Inventory : Popup
{

//    [SerializeField] private GameObject ceil;
    [SerializeField] private GameObject inventoryCharacterItemPrefab;
//    [SerializeField] private ScrollRect scrollView;

//    private List<InventoryCeil> _ceils = new List<InventoryCeil>();
    private Dictionary<string, InventoryCharacter> characters = new Dictionary<string, InventoryCharacter>();
    private Character _currentCharacter;

    //    private Dictionary<string, Dictionary<int, CharacterItemData>> _inventoryCeils = new Dictionary<string, Dictionary<int, CharacterItemData>>();
    //    public readonly Dictionary<string, Item> InventoryItems = new Dictionary<string, Item>();

//    public readonly Dictionary<string, Dictionary<string, Item>> InventoryItems = new Dictionary<string, Dictionary<string, Item>>();
//    private readonly Dictionary<string, Dictionary<int, string>> _inventoryCeils = new Dictionary<string, Dictionary<int, string>>();

    private readonly Dictionary<string, int> _waitingAddItemCeils = new Dictionary<string, int>();
    private readonly Dictionary<string, int> _waitingRemoveItemCeils = new Dictionary<string, int>();
    private ICommonLoader<GameObject> _gameObjectLoader;
    private IPathDataProvider _pathProvider;


    protected override void OnEnable()
    {
        Init(GameManager.Instance.CurrentCharacter); //todo
        _gameObjectLoader = AllServices.Container.Single<ILoaderService>().GameObjectLoader;
        _pathProvider = AllServices.Container.Single<ILoaderService>().PathDataProvider;

        base.OnEnable();
    }

    public Dictionary<int, CharacterItemData> GetInventoryCeils(string characterId)
    {
        InventoryCharacter characteInv;
        if (characters.TryGetValue(characterId, out characteInv))
            return characteInv.GetInventoryCeils();

        return null;
    }

    public Dictionary<string, Item> GetCharacterItems(string characterId)
    {
        InventoryCharacter characteInv;
        if (characters.TryGetValue(characterId, out characteInv))
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
            if (!characters.ContainsKey(characterInfo.guid))
            {
                var item = AddChild(inventoryCharacterItemPrefab).GetComponent<InventoryCharacter>();
                characters[characterInfo.guid] = item;
            }
            characters[characterInfo.guid].Init(characterInfo);
        }

        //Remove old
        var keys = characters.Keys;
        foreach (var characterInventoryGuid in keys)
        {
            if (!GameManager.Instance.Characters.ContainsKey(characterInventoryGuid))
            {
                Destroy(characters[characterInventoryGuid].gameObject);
                characters.Remove(characterInventoryGuid);
            }
        }

        _currentCharacter.OnAddToInventory += AddToInventoryHandler;
        _currentCharacter.OnRemoveFromInventory += RemoveFromInventoryHandler;
        InnerUpdate();
    }

    //    public void AddToInvetory(Item item)
    //    {
    //        if (_currentCharacter == null) return;
    //        _currentCharacter.AddToInvetory(item);
    //    }


    public bool AddToInvetory(Item item)
    {
        if (_currentCharacter == null) return false;
        if (!characters.ContainsKey(_currentCharacter.guid)) return false;
        return  characters[_currentCharacter.guid].AddItem(item);
    }

    public bool AddToInvetory(PickUpItem pickUpItem)
    {
        var item = pickUpItem.Item;
        if (item == null) return false;

        if (!AddToInvetory(item)) return false;
        Destroy(pickUpItem.gameObject);
        return true;

    }

    public bool AddToInvetory(Item item, InventoryCeil ceil)
    {
        if (ceil == null)
        {
            return AddToInvetory(item);
        }

        var ceil1Character = ceil.GetComponentInParent<InventoryCharacter>();
        return ceil1Character.AddItem(item, ceil.Index);
    }

    public void SetItemToInvetory(string characterGuid, Item item, int ceilIndex)
    {
        if (!characters.ContainsKey(characterGuid))
            characters[characterGuid] = AddChild(inventoryCharacterItemPrefab).GetComponent<InventoryCharacter>(); ;

        characters[characterGuid].SetItem(item, ceilIndex);
    }

 

    public void RemoveFromInvetory(InventoryCeil ceil)
    {
        var item = ceil.Item;
        if (item == null) return;

        var ceil1Character = ceil.GetComponentInParent<InventoryCharacter>();
        ceil1Character.RemoveItem(item.Guid, ceil.Index);
    }

    public void RemoveFromInvetoryToGround(InventoryCeil ceil, Vector3 groundPosition)
    {
        var item = ceil.Item;
        RemoveFromInvetory(ceil);
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
        if (_waitingAddItemCeils.Count != 0)
        {
            foreach (var pair in _waitingAddItemCeils)
            {
                characters[pair.Key].AddItem(item, pair.Value);
                break;
            }
            _waitingAddItemCeils.Clear();
            return;
        }

        characters[characterId].AddItem(item);
    }


    private void RemoveFromInventoryHandler(string characterId, Item item)
    {
        _waitingAddItemCeils.Clear();

        if (_waitingRemoveItemCeils.Count != 0)
        {
            foreach (var pair in _waitingRemoveItemCeils)
            {
                characters[pair.Key].RemoveItem(item.Guid);
                break;
            }
            _waitingRemoveItemCeils.Clear();
            return;
        }

        characters[characterId].RemoveItem(item.Guid);
    }

    public void SwapCeils(ItemCeil ceil1, ItemCeil ceil2)
    {
        var ceil1Character = ceil1.GetComponentInParent<InventoryCharacter>();
        var ceil2Character = ceil2.GetComponentInParent<InventoryCharacter>();

        if (ceil1Character.CharacterGuid == ceil2Character.CharacterGuid)
        {
            ceil1Character.SwapCeils(ceil1, ceil2);
            return;
        }

        var ceil1Item = ceil1.Item;
        var ceil2Item = ceil2.Item;
        if (ceil1Item != null) ceil1Character.RemoveItem(ceil1Item.Guid);
        if (ceil2Item != null) ceil2Character.RemoveItem(ceil2Item.Guid);

        if (ceil2Item != null) ceil1Character.AddItem(ceil2Item, ceil1.Index);
        if (ceil1Item != null) ceil2Character.AddItem(ceil1Item, ceil2.Index);
    }


    //todo work only with current character
    public void UnEquipItemToCeil(EquipItem equipItem, ItemCeil ceil)
    {
        var ceilCharacter = ceil.GetComponentInParent<InventoryCharacter>();
        _waitingAddItemCeils[ceilCharacter.CharacterGuid] = ceil.Index;

        ItemManager.Instance.UnEquipItem(equipItem);
        AddToInvetory(equipItem);
    }


    public bool CanEquip(EquipItem item)
    {
        return true;
    }


    public void EquipItem(EquipItem equipItem, ItemCeil ceil)
    {
        var ceilCharacter = ceil.GetComponentInParent<InventoryCharacter>();
        _waitingRemoveItemCeils[ceilCharacter.CharacterGuid] = ceil.Index;
        _waitingAddItemCeils[ceilCharacter.CharacterGuid] = ceil.Index;

        ItemManager.Instance.EquipItem(equipItem);
    }
}
