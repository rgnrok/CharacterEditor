using UnityEngine;

namespace CharacterEditor
{
    namespace Mesh
    {
        public class FaceFeature : AbstractMesh
        {
            public override bool IsFaceMesh => true;

            public FaceFeature(IMeshLoader loader, Transform anchor, string characterRace):base(loader, anchor, characterRace, MeshType.FaceFeature, GetMergeOrder())
            {
            }

            public static int GetMergeOrder()
            {
                return 2;
            }
        }
    }
}