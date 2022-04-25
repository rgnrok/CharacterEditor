using UnityEngine;

namespace CharacterEditor
{
    namespace Mesh
    {
        public class Belt : AbstractMesh
        {
            public Belt(IMeshLoader loader, Transform anchor, string characterRace) : base(loader, anchor, characterRace, MeshType.Belt, GetMergeOrder()) {
            }
         
            public static int GetMergeOrder() {
                return 9;
            }
        }
    }
}