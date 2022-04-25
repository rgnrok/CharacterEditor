using UnityEngine;

namespace CharacterEditor
{
    namespace Mesh
    {
        public class Beard : AbstractMesh
        {
            public override bool IsFaceMesh => true;

            public Beard(IMeshLoader loader, Transform anchor, string characterRace):base(loader, anchor, characterRace, MeshType.Beard, GetMergeOrder())
            {
            }

            public static int GetMergeOrder()
            {
                return 1;
            }
        }
    }
}