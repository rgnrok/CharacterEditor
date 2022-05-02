using System;
using System.Collections.Generic;

namespace CharacterEditor.Helpers
{
    public static class EnumExtension
    {
        public static TEnum[] GetEnumValues<TEnum>() where TEnum : Enum
        {
            return (TEnum[]) Enum.GetValues(typeof(TEnum));
        }

        public static TEnum[] FlagToArray<TEnum>(this Enum mask) where TEnum : Enum
        {
            var list = new List<TEnum>();
            var intMask = Convert.ToInt32(mask);
            foreach (var enumValue in Enum.GetValues(typeof(TEnum)))
            {
                var checkBit = intMask & (int) enumValue;
                if (checkBit != 0)
                    list.Add((TEnum) enumValue);
            }

            return list.ToArray();
        }
    }
}