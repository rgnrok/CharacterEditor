using UnityEngine;

namespace CharacterEditor
{
    namespace Mesh
    {
        public class Arm : AbstractMesh
        {
            public Arm(IMeshLoader loader, Transform anchor, string characterRace, MeshType type) : base(loader, anchor, characterRace, type, GetMergeOrder(type)) {
            }

            public static int GetMergeOrder(MeshType type)
            {
                return type == MeshType.ArmLeft ? 6 : 7;
            }
        }
    }
}