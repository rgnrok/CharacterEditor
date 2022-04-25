namespace CharacterEditor
{
    namespace CharacterInventory
    {
        public class FaceFeature : ItemMesh
        {
            public FaceFeature(IMeshLoader loader, string meshPath, MeshType type) : base(loader, meshPath, null, type, GetMerheOrder(type))
            {
            }

            public static int GetMerheOrder(MeshType type)
            {
                return 2;
            }
        }
    }
}