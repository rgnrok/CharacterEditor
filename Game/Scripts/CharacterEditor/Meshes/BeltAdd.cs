using UnityEngine;

namespace CharacterEditor
{
    namespace Mesh
    {
        public class BeltAdd : AbstractMesh
        {
            public BeltAdd(IMeshLoader loader, Transform anchor, string characterRace) : base(loader, anchor, characterRace, MeshType.BeltAdd, GetMergeOrder()) {
            }

            public static int GetMergeOrder()
            {
                return 10;
            }
        }
    }
}