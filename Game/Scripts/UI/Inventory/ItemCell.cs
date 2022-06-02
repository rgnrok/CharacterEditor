using System;
using System.Collections;
using System.Collections.Generic;
using CharacterEditor;
using CharacterEditor.CharacterInventory;
using CharacterEditor.Services;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemCell : MonoBehaviour, IPointerClickHandler {

    [SerializeField] private Image itemImage;

    public Item Item { get; private set; }
    public int Index { get; set; }

    public Action<int, Item, Item> UpdateItem;
    private ISpriteLoader _spriteLoader;
    private IPathDataProvider _pathProvider;

    private bool _isInit;
    private bool _isDisabled;

    private void Awake()
    {
        Init();

        UpdateImage();
    }

    protected virtual void Init()
    {
        _spriteLoader = AllServices.Container.Single<ILoaderService>().SpriteLoader;
        _pathProvider = AllServices.Container.Single<ILoaderService>().PathDataProvider;

        _isInit = true;
    }

    public virtual void SetItem(Item item, bool disabled = false)
    {
        _isDisabled = disabled;

        var oldItem = Item;
        Item = item;
        UpdateImage();


        UpdateItem?.Invoke(Index, oldItem, Item);
    }

    public bool IsEmpty()
    {
        return Item == null;
    }

    protected void UpdateImage()
    {
        if (!_isInit) return;
        if (itemImage == null) return;
        if (Item == null)
        {
            ChangeItemVisible(false);
            return;
        }

        _spriteLoader.LoadItemIcon(_pathProvider.GetPath(Item.Data.icon),
            icon =>
            {
                itemImage.sprite = icon;
                itemImage.color = _isDisabled ? Color.black : Color.white;
                ChangeItemVisible(true);
            });
    }

    protected virtual void ChangeItemVisible(bool visible)
    {
        itemImage.transform.parent.gameObject.SetActive(visible);
    }


    public void OnPointerClick(PointerEventData eventData)
    {
        OnClickHandler();
    }

    protected virtual void OnClickHandler()
    {

    }
}
