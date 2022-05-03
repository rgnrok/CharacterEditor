using System;

namespace CharacterEditor
{
    public enum TextureType
    {
        Undefined = 0x0000,
        //Skin
        Skin = 0x0001,
        Eye = 0x0002,
        Eyebrow = 0x0004,
        Beard = 0x0008,
        Hair = 0x0010,
        Scar = 0x0020,
        FaceFeature = 0x0040,

        //Clothes
        Head = 0x0080,
        Pants = 0x0100,
        Torso = 0x0200,
        Shoe = 0x0400,
        Glove = 0x0800,
        Belt = 0x1000,
        RobeLong = 0x2000,
        RobeShort = 0x4000,
        Cloak = 0x8000,
    }
}
