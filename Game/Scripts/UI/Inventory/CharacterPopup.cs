using System;
using CharacterEditor.CharacterInventory;
using CharacterEditor.Services;
using UnityEngine;

namespace CharacterEditor
{
    public class CharacterPopup : Popup
    {
        [SerializeField] private RenderTexture characterTexture;

        [SerializeField] private EquipPanelCell _headCell;
        [SerializeField] private EquipPanelCell _armorCell;
        [SerializeField] private EquipPanelCell _pantsCell;
        [SerializeField] private EquipPanelCell _glovesCell;
        [SerializeField] private EquipPanelCell _bootsCell;
        [SerializeField] private EquipPanelCell _cloakCell;
        [SerializeField] private EquipPanelCell _beltCell;
        [SerializeField] private EquipPanelCell _rightHandCell;
        [SerializeField] private EquipPanelCell _leftHandCell;

        private Character _currentCharacter;
        private ICharacterEquipItemService _itemManager;
        private GameManager _gameManager;

        private PointerYRotateComponent _characterRotateComponent;

        void Awake()
        {
            _itemManager = AllServices.Container.Single<ICharacterEquipItemService>();
            _gameManager = GameManager.Instance;

            _characterRotateComponent = GetComponentInChildren<PointerYRotateComponent>();
        }

        void OnDisable()
        {
            if (_itemManager != null)
            {
                _itemManager.OnEquip -= OnEquipHandler;
                _itemManager.OnUnEquip -= OnUnEquipHandler;
            }

            if (_gameManager != null)
                _gameManager.OnChangeCharacter -= OnChangeCharacterHandler;

            if (_currentCharacter.GameObjectData.PreviewCharacterObject != null)
                _currentCharacter.GameObjectData.PreviewCharacterObject.SetActive(false);
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            _itemManager.OnEquip += OnEquipHandler;
            _itemManager.OnUnEquip += OnUnEquipHandler;
            _gameManager.OnChangeCharacter += OnChangeCharacterHandler;

            Init(_gameManager.CurrentCharacter); 
        }

        private void OnChangeCharacterHandler(Character character)
        {
            if (_currentCharacter.GameObjectData.PreviewCharacterObject != null)
                _currentCharacter.GameObjectData.PreviewCharacterObject.SetActive(false);

            Init(character);
        }

        private void Init(Character character)
        {
            _currentCharacter = character;

            var characterPreview = _currentCharacter.GameObjectData.PreviewCharacterObject;
            var previewCamera = characterPreview.GetComponentInChildren<Camera>();
            previewCamera.targetTexture = characterTexture;

            characterPreview.SetActive(true);
            _characterRotateComponent.SetTarget(characterPreview.transform.Find("Model"));

            UpdateEquipItems();
        }

        private void OnEquipHandler(EquipItem item)
        {
            UpdateEquipItems();
        }

        private void OnUnEquipHandler(EquipItem item)
        {
            UpdateEquipItems();
        }

        private void UpdateEquipItems()
        {
            foreach (EquipItemSlot slotType in Enum.GetValues(typeof(EquipItemSlot)))
            {
                var ceil = GetCeil(slotType);
                if (ceil == null) continue;

                _currentCharacter.EquipItems.TryGetValue(slotType, out var item);
                ceil.SetItem(item);
            }

            UpdateTwoHandCeil();
        }

        private void UpdateTwoHandCeil()
        {
            _currentCharacter.EquipItems.TryGetValue(EquipItemSlot.HandRight, out var item);
            
            if (item != null && item.ItemSubType == EquipItemSubType.TwoHand) _leftHandCell.SetItem(item, true);
        }

        private EquipPanelCell GetCeil(EquipItemSlot slot)
        {
            switch (slot)
            {
                case EquipItemSlot.Helm:
                    return _headCell;
                case EquipItemSlot.Armor:
                    return _armorCell;
                case EquipItemSlot.Pants:
                    return _pantsCell;
                case EquipItemSlot.Gloves:
                    return _glovesCell;
                case EquipItemSlot.Boots:
                    return _bootsCell;
                case EquipItemSlot.Belt:
                    return _beltCell;
                case EquipItemSlot.Cloak:
                    return _cloakCell;
                case EquipItemSlot.HandRight:
                    return _rightHandCell;
                case EquipItemSlot.HandLeft:
                    return _leftHandCell;
                default:
                    return null;
            }
        }
    }
}