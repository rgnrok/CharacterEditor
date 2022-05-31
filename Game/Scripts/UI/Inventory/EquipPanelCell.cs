﻿using System.Collections.Generic;
using CharacterEditor;
using CharacterEditor.CharacterInventory;
using CharacterEditor.Services;
using UnityEngine;

public class EquipPanelCell : ItemCell
{
    [SerializeField] private EquipItemSlot _slot;
    [SerializeField] private EquipItemType[] _availableItemTypes;
    private ICharacterEquipItemService _characterEquipItemService;

    public EquipItemSlot ItemSlot { get { return _slot; } }
    public EquipItemType[] AvailableTypes { get { return _availableItemTypes; } }

    private void Awake()
    {
        _characterEquipItemService = AllServices.Container.Single<ICharacterEquipItemService>();
    }

    protected override void OnClickHandler()
    {
        if (Item is EquipItem equipItem)
        {
            _characterEquipItemService.UnEquipItem(equipItem);
            GameManager.Instance.Inventory.AddToInventory(equipItem);
            SetItem(null);
        }
    }
}