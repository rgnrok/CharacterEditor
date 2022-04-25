namespace CharacterEditor
{
    namespace CharacterInventory
    {
        public class FaceMeshFactory
        {
            public static ItemMesh Create(MeshType meshType, IMeshLoader loader, string meshPath)
            {
                switch (meshType)
                {
                    case MeshType.Hair:
                        return new Hair(loader, meshPath, meshType);
                    case MeshType.Beard:
                        return new Beard(loader, meshPath, meshType);
                    case MeshType.FaceFeature:
                        return new FaceFeature(loader, meshPath, meshType);
                }
                return null;
            }
        }
    }
}