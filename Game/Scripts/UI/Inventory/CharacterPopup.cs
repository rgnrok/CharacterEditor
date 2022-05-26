using System;
using CharacterEditor.CharacterInventory;
using UnityEngine;

namespace CharacterEditor
{
    public class CharacterPopup : Popup
    {
        [SerializeField] private RenderTexture characterTexture;

        [SerializeField] private EquipPanelCeil headCeil;
        [SerializeField] private EquipPanelCeil armorCeil;
        [SerializeField] private EquipPanelCeil pantsCeil;
        [SerializeField] private EquipPanelCeil glovesCeil;
        [SerializeField] private EquipPanelCeil bootsCeil;
        [SerializeField] private EquipPanelCeil cloakCeil;
        [SerializeField] private EquipPanelCeil beltCeil;
        [SerializeField] private EquipPanelCeil rightHandCeil;
        [SerializeField] private EquipPanelCeil leftHandCeil;

        private Character _currentCharacter;
        private ItemManager _itemManager;
        private GameManager _gameManager;

        private PointerYRotateComponent _characterRotateComponent;

        void Awake()
        {
            _itemManager = ItemManager.Instance;
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
            
            if (item != null && item.ItemSubType == EquipItemSubType.TwoHand) leftHandCeil.SetItem(item, true);
        }

        private EquipPanelCeil GetCeil(EquipItemSlot slot)
        {
            switch (slot)
            {
                case EquipItemSlot.Helm:
                    return headCeil;
                case EquipItemSlot.Armor:
                    return armorCeil;
                case EquipItemSlot.Pants:
                    return pantsCeil;
                case EquipItemSlot.Gloves:
                    return glovesCeil;
                case EquipItemSlot.Boots:
                    return bootsCeil;
                case EquipItemSlot.Belt:
                    return beltCeil;
                case EquipItemSlot.Cloak:
                    return cloakCeil;
                case EquipItemSlot.HandRight:
                    return rightHandCeil;
                case EquipItemSlot.HandLeft:
                    return leftHandCeil;
                default:
                    return null;
            }
        }
    }
}