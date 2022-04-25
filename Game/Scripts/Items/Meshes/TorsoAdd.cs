namespace CharacterEditor
{
    namespace CharacterInventory
    {
        public class TorsoAdd : ItemMesh
        {
            public TorsoAdd(IMeshLoader loader, string meshPath, string texturePath) : base(loader, meshPath,
                texturePath, MeshType.TorsoAdd, 3)
            {
            }
        }
    }
}