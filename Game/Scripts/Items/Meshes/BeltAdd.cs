namespace CharacterEditor
{
    namespace CharacterInventory
    {
        public class BeltAdd : ItemMesh
        {
            public BeltAdd(IMeshLoader loader, string meshPath, string texturePath) : base(loader, meshPath,
                texturePath, MeshType.BeltAdd, 10)
            {
            }
        }
    }
}