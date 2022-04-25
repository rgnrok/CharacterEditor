using System;
using System.Collections;
using System.Collections.Generic;
using CharacterEditor;
using CharacterEditor.CharacterInventory;
using CharacterEditor.Services;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemCeil : MonoBehaviour, IPointerClickHandler {

    [SerializeField] private Image itemImage;

    public Item Item { get; private set; }
    public int Index { get; set; }

    public Action<int, Item, Item> UpdateItem;
    private IIconLoader _iconLoader;

    void Awake()
    {
        UpdateImage();
        _iconLoader = AllServices.Container.Single<ILoaderService>().IconLoader;

    }

    public virtual void SetItem(Item item, bool disabled = false)
    {
        var oldItem = Item;
        Item = item;
        UpdateImage(disabled);

        if (UpdateItem != null) UpdateItem(Index, oldItem, Item);
    }

    public bool IsEmpty()
    {
        return Item == null;
    }

    protected void UpdateImage(bool disabled = false)
    {
        if (itemImage == null) return;
        if (Item == null)
        {
            ChangeItemVisible(false);
            return;
        }

        _iconLoader.LoadItemIcon(Item.Data.iconBundleName,
            icon =>
            {
                itemImage.sprite = icon;
                itemImage.color = disabled ? Color.black : Color.white;
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
