namespace CharacterEditor
{
    namespace CharacterInventory
    {
        public enum EquipItemType
        {
            Undefined,
            Armor,
            Weapon,
            Shield,
            Gloves,
            Boots,
            Pants,
            Belt,
            Helm,
            Cloak,
        }

        public enum EquipItemSlot
        {
            Undefined,
            Armor,
            HandRight,
            HandLeft,
            Gloves,
            Boots,
            Pants,
            Belt,
            Helm,
            Cloak
        }

        public enum EquipItemSubType
        {
            Undefined,
            Pants,
            LongRobe,
            ShortRobe,
            TwoHand,
            OneHand,
            Bow,
        }

        public enum EquipItemPantsSubType
        {
            Undefined,
            Pants = EquipItemSubType.Pants,
            LongRobe = EquipItemSubType.LongRobe,
            ShortRobe = EquipItemSubType.ShortRobe,
        }
        public enum EquipItemWeaponSubType
        {
            Undefined,
            TwoHand = EquipItemSubType.TwoHand,
            OneHand = EquipItemSubType.OneHand,
            Bow = EquipItemSubType.Bow,
        }
    }
}