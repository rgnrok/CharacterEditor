namespace CharacterEditor
{
    namespace CharacterInventory
    {
        public class ArmorMeshFactory
        {
            public static ItemMesh Create(MeshType meshType, IMeshLoader loader, string meshPath, string texturePath)
            {
                switch (meshType)
                {
                    case MeshType.HandLeft:
                    case MeshType.HandRight:
                    case MeshType.ArmLeft:
                    case MeshType.ArmRight:
                    case MeshType.ShoulderLeft:
                    case MeshType.ShoulderRight:
                    case MeshType.LegLeft:
                    case MeshType.LegRight:
                    case MeshType.Torso:
                    case MeshType.TorsoAdd:
                    case MeshType.Helm:
                    case MeshType.Belt:
                    case MeshType.BeltAdd:
                        return new ItemMesh(meshType, loader, meshPath, texturePath);
                }
                return null;
            }
        }
    }
}