namespace CharacterEditor
{
    namespace CharacterInventory
    {
        public class Belt : ItemMesh
        {
            public Belt(IMeshLoader loader, string meshPath, string texturePath) : base(loader, meshPath,
                texturePath, MeshType.Belt, 9)
            {
            }
        }
    }
}