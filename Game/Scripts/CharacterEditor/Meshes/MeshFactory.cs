using CharacterEditor.Mesh;
using UnityEngine;

namespace CharacterEditor
{
    public class MeshFactory
    {
        public static AbstractMesh Create(IMeshLoader loader, MeshType meshType, Transform anchor, string characterRace)
        {
            switch (meshType)
            {
                case MeshType.Beard:
                    return new Beard(loader, anchor, characterRace);
                case MeshType.FaceFeature:
                    return new FaceFeature(loader, anchor, characterRace);
                case MeshType.Hair:
                    return new Hair(loader, anchor, characterRace);
                case MeshType.Helm:
                    return new Helm(loader, anchor, characterRace);
                case MeshType.Torso:
                    return new Torso(loader, anchor, characterRace);
                case MeshType.TorsoAdd:
                    return new TorsoAdd(loader, anchor, characterRace);
                case MeshType.LegLeft:
                    return new Leg(loader, anchor, characterRace, MeshType.LegLeft);
                case MeshType.LegRight:
                    return new Leg(loader, anchor, characterRace, MeshType.LegRight);
                case MeshType.ShoulderLeft:
                    return new Shoulder(loader, anchor, characterRace, MeshType.ShoulderLeft);
                case MeshType.ShoulderRight:
                    return new Shoulder(loader, anchor, characterRace, MeshType.ShoulderRight);
                case MeshType.ArmLeft:
                    return new Arm(loader, anchor, characterRace, MeshType.ArmLeft);
                case MeshType.ArmRight:
                    return new Arm(loader, anchor, characterRace, MeshType.ArmRight);
                case MeshType.Belt:
                    return new Belt(loader, anchor, characterRace);
                case MeshType.BeltAdd:
                    return new BeltAdd(loader, anchor, characterRace);
                case MeshType.HandLeft:
                    return new Hand(loader, anchor, characterRace, MeshType.HandLeft);
                case MeshType.HandRight:
                    return new Hand(loader, anchor, characterRace, MeshType.HandRight);
                default:
                    return null;
            }
        }
    }
}
