using UnityEngine;

namespace CharacterEditor
{
    namespace Mesh
    {
        public class Hair : AbstractMesh
        {
            public override bool IsFaceMesh => true;

            public Hair(IMeshLoader loader, Transform anchor, string characterRace) : base(loader, anchor, characterRace, MeshType.Hair, GetMergeOrder())
            {

            }

            public static int GetMergeOrder()
            {
                return 0;
            }
        }
    }
}