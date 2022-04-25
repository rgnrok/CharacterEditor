namespace CharacterEditor
{
    namespace CharacterInventory
    {
        public class Hair : ItemMesh
        {
            public Hair(IMeshLoader loader, string meshPath, MeshType type) : base(loader, meshPath, null, type, GetMerheOrder(type))
            {
            }

            public static int GetMerheOrder(MeshType type)
            {
                return 0;
            }
        }
    }
}