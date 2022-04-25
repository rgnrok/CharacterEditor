using UnityEngine;

namespace CharacterEditor
{
    namespace Mesh
    {
        public class TorsoAdd : AbstractMesh
        {
            public TorsoAdd(IMeshLoader loader, Transform anchor, string characterRace) : base(loader, anchor, characterRace, MeshType.TorsoAdd, GetMergeOrder()) {
            }

            public static int GetMergeOrder() {
                return 3;
            }
        }
    }
}