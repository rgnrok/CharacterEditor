using UnityEngine;

namespace CharacterEditor
{
    namespace Mesh
    {
        public class Hand : AbstractMesh
        {
            public Hand(IMeshLoader loader, Transform anchor, string characterRace, MeshType type) : base(loader, anchor, characterRace, type, GetMergeOrder(type)) {
            }

            public static int GetMergeOrder(MeshType type) {
                return type == MeshType.HandLeft ? 0 : 1;
            }
        }
    }
}