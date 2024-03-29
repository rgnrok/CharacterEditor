﻿using CharacterEditor;
using CharacterEditor.CharacterInventory;
using CharacterEditor.Services;
using UnityEngine;

public class EquipPanelCell : ItemCell
{
    [SerializeField] private EquipItemSlot _slot;
    [SerializeField] private EquipItemType[] _availableItemTypes;
    [SerializeField] private GameObject _background;
    private ICharacterEquipItemService _characterEquipItemService;

    public EquipItemSlot ItemSlot => _slot;
    public EquipItemType[] AvailableTypes => _availableItemTypes;

    protected override void Init()
    {
        base.Init();
        _characterEquipItemService = AllServices.Container.Single<ICharacterEquipItemService>();
    }

    protected override void OnClickHandler()
    {
        if (!(Item is EquipItem equipItem)) return;

        _characterEquipItemService.UnEquipItem(equipItem);
        GameManager.Instance.Inventory.AddToInventory(equipItem);
        SetItem(null);
    }

    public override void SetItem(Item item, bool disabled = false)
    {
        base.SetItem(item, disabled);
        _background.SetActive(item == null);
    }
}