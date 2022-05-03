using System;

namespace CharacterEditor
{
    public enum MeshType
    {
        Undefined = 0x0000,
        Hair = 0x0001,
        Beard = 0x0002,
        FaceFeature = 0x0004,
        Helm = 0x0008,
        Torso = 0x0010,
        Belt = 0x0020,
        ShoulderRight = 0x0040,
        ShoulderLeft = 0x0080,
        ArmRight = 0x0100,
        ArmLeft = 0x0200,
        LegRight = 0x0400,
        LegLeft = 0x0800,
        HandRight = 0x1000,
        HandLeft = 0x2000,
        TorsoAdd = 0x4000,
        BeltAdd = 0x8000,
    }
}
