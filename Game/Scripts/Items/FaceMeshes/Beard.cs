namespace CharacterEditor
{
    namespace CharacterInventory
    {
        public class Beard : ItemMesh
        {
            public Beard(IMeshLoader loader, string meshPath, MeshType type) : base(loader, meshPath, null, type, GetMerheOrder(type))
            {
            }

            public static int GetMerheOrder(MeshType type)
            {
                return 1;
            }
        }
    }
}