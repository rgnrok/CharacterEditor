using UnityEngine;

namespace CharacterEditor
{
    namespace Mesh
    {
        public class Leg : AbstractMesh
        {
            public Leg(IMeshLoader loader, Transform anchor, string characterRace, MeshType type) : base(loader, anchor, characterRace, type, GetMergeOrder(type)) {
            }

            public static int GetMergeOrder(MeshType type)
            {
                return type == MeshType.LegLeft ? 11 : 12;
            }
        }
    }
}