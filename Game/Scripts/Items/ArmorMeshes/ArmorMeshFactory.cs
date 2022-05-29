namespace CharacterEditor
{
    namespace CharacterInventory
    {
        public class ArmorMeshFactory
        {
            public static ItemMesh Create(MeshType meshType, IMeshLoader loader, string meshPath, ITextureLoader textureLoader, string texturePath)
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
                        var itemTexture = new ItemTexture(textureLoader, texturePath);
                        return new ItemMesh(meshType, loader, meshPath, itemTexture);
                }
                return null;
            }
        }
    }
}