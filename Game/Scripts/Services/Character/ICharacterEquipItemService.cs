using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CharacterEditor.CharacterInventory;
using UnityEngine;

namespace CharacterEditor.Services
{
    public interface ICharacterEquipItemService : IService, ICleanable
    {
        void SetCharacter(Character character);
        bool CanEquip(EquipItem item);
        void UnEquipItem(EquipItem item);
        Task EquipItem(EquipItem item, EquipItemSlot slotType = EquipItemSlot.Undefined);
        Task EquipItems(Dictionary<EquipItemSlot, EquipItem> equipItems);
        void SwapEquippedItem(EquipItemSlot slotType1, EquipItemSlot slotType2);
        event Action<EquipItem> OnEquip;
        event Action<EquipItem> OnUnEquip;
        Texture2D CharacterTexture { get; }
        Texture2D ArmorTexture { get; }
        event Action OnTexturesChanged;
        Task LoadMaterials();
    }
}