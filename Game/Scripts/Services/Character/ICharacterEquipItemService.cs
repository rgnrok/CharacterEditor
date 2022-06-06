using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CharacterEditor.CharacterInventory;
using UnityEngine;

namespace CharacterEditor.Services
{
    public interface ICharacterEquipItemService : IService, ICleanable
    {
        void SetupCharacter(Character character);
        bool CanEquip(EquipItem item);
        void UnEquipItem(EquipItem item);
        void EquipItem(EquipItem item, EquipItemSlot slotType = EquipItemSlot.Undefined);
        Task EquipItems(Character character, Dictionary<EquipItemSlot, EquipItem> equipItems);
        void SwapEquippedItem(EquipItemSlot slotType1, EquipItemSlot slotType2);

        event Action<EquipItem> OnEquip;
        event Action<EquipItem> OnUnEquip;

        Texture2D GetCurrentCharacterTexture();
        Texture2D GetCurrentCharacterArmorTexture();
        event Action OnTexturesChanged;
        Task LoadMaterials();
    }
}