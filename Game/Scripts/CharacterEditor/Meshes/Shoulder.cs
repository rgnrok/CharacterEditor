using UnityEngine;

namespace CharacterEditor
{
    namespace Mesh
    {
        public class Shoulder : AbstractMesh
        {
            public Shoulder(IMeshLoader loader, Transform anchor, string characterRace, MeshType meshType) : base(loader, anchor, characterRace, meshType, GetMergeOrder(meshType)) {
            }
            
            public static int GetMergeOrder(MeshType type)
            {
                return type == MeshType.ShoulderLeft ? 4 : 5;
            }
        }
    }
}