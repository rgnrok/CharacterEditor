using UnityEngine;

namespace CharacterEditor
{
    namespace Mesh
    {
        public class Torso : AbstractMesh
        {
            public Torso(IMeshLoader loader, Transform anchor, string characterRace) : base(loader, anchor, characterRace, MeshType.Torso, GetMergeOrder()) {
            }

            public static int GetMergeOrder()
            {
                return 2;
            }
        }
    }
}