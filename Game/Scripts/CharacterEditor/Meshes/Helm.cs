using UnityEngine;

namespace CharacterEditor
{
    namespace Mesh
    {
        public class Helm : AbstractMesh
        {
            public Helm(IMeshLoader loader, Transform anchor, string characterRace) : base(loader, anchor, characterRace, MeshType.Helm, GetMergeOrder()) {
            }
          
            public static int GetMergeOrder() {
                return 8;
            }
        }
    }
}