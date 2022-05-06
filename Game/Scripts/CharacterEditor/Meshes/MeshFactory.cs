using System;
using CharacterEditor.Mesh;

namespace CharacterEditor
{
    public class MeshFactory
    {
        public static CharacterMesh Create(IMeshLoader loader, IDataManager dataManager, MeshType meshType, string characterRace)
        {
            var meshTexturesPath = dataManager.ParseCharacterMeshes(characterRace, meshType);
            var meshOrder = GetMeshMergeOrder(meshType);
            switch (meshType)
            {
                case MeshType.Beard:
                case MeshType.FaceFeature:
                case MeshType.Hair:
                    return new CharacterMesh(loader, meshTexturesPath, meshType, meshOrder, true);
                default:
                    return new CharacterMesh(loader, meshTexturesPath, meshType, meshOrder, false);
            }
        }

        public static int GetMeshMergeOrder(MeshType type)
        {
            switch (type)
            {
                //Face
                case MeshType.Hair:
                    return 0;
                case MeshType.Beard:
                    return 1;
                case MeshType.FaceFeature:
                    return 2;
                
                //Armor
                case MeshType.HandLeft:
                    return 0;
                case MeshType.HandRight:
                    return 1;
                case MeshType.Torso:
                    return 2;
                case MeshType.TorsoAdd:
                    return 3;
                case MeshType.ShoulderLeft:
                    return 4;
                case MeshType.ShoulderRight:
                    return 5;
                case MeshType.ArmLeft:
                    return 6;
                case MeshType.ArmRight:
                    return 7;
                case MeshType.Helm:
                    return 8;
                case MeshType.Belt:
                    return 9;
                case MeshType.BeltAdd:
                    return 10;
                case MeshType.LegLeft:
                    return 11;
                case MeshType.LegRight:
                    return 12;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
    }
}
