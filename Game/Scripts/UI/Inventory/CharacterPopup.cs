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

      

        void OnDisable()
        {
            ItemManager.Instance.OnEquip -= OnEquipHandler;
            ItemManager.Instance.OnUnEquip -= OnUnEquipHandler;
            GameManager.Instance.OnChangeCharacter -= OnChangeCharacterHandler;

            if (_currentCharacter.GameObjectData.PreviewCharacterObject != null)
                _currentCharacter.GameObjectData.PreviewCharacterObject.SetActive(false);
        }

        void OnEnable()
        {
            ItemManager.Instance.OnEquip += OnEquipHandler;
            ItemManager.Instance.OnUnEquip += OnUnEquipHandler;
            GameManager.Instance.OnChangeCharacter += OnChangeCharacterHandler;
            Init(GameManager.Instance.CurrentCharacter); 
        }

        private void OnChangeCharacterHandler(Character character)
        {
            if (_currentCharacter.GameObjectData.PreviewCharacterObject != null)
                _currentCharacter.GameObjectData.PreviewCharacterObject.SetActive(false);

            Init(character);
        }

        public void Init(Character character)
        {
            _currentCharacter = character;

            var previewCamera = _currentCharacter.GameObjectData.PreviewCharacterObject.GetComponentInChildren<Camera>();
            previewCamera.targetTexture = characterTexture;

            if (_currentCharacter.GameObjectData.PreviewCharacterObject != null)
                _currentCharacter.GameObjectData.PreviewCharacterObject.SetActive(true);

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

                EquipItem item;
                _currentCharacter.EquipItems.TryGetValue(slotType, out item);
                ceil.SetItem(item);
            }

            UpdateTwoHandCeil();
        }

        private void UpdateTwoHandCeil()
        {
            EquipItem item;
            _currentCharacter.EquipItems.TryGetValue(EquipItemSlot.HandRight, out item);
            
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