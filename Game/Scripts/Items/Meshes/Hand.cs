namespace CharacterEditor
{
    namespace CharacterInventory
    {
        public class Hand : ItemMesh
        {
            public Hand(IMeshLoader loader, string meshPath, string texturePath, MeshType type) : base(loader, meshPath,
                texturePath, type, type == MeshType.HandLeft ? 0 : 1)
            {
            }
        }
    }
}