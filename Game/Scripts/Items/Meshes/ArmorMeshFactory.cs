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
                        return new Hand(loader, meshPath, texturePath, meshType);
                    case MeshType.ArmLeft:
                    case MeshType.ArmRight:
                        return new Arm(loader, meshPath, texturePath, meshType);
                    case MeshType.ShoulderLeft:
                    case MeshType.ShoulderRight:
                        return new Shoulder(loader, meshPath, texturePath, meshType);
                    case MeshType.LegLeft:
                    case MeshType.LegRight:
                        return new Leg(loader, meshPath, texturePath, meshType);
                    case MeshType.Torso:
                        return new Torso(loader, meshPath, texturePath);
                    case MeshType.TorsoAdd:
                        return new TorsoAdd(loader, meshPath, texturePath);
                    case MeshType.Helm:
                        return new Helm(loader, meshPath, texturePath);
                    case MeshType.Belt:
                        return new Belt(loader, meshPath, texturePath);
                    case MeshType.BeltAdd:
                        return new BeltAdd(loader, meshPath, texturePath);
                }
                return null;
            }
        }
    }
}