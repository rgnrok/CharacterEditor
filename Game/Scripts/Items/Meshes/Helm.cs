namespace CharacterEditor
{
    namespace CharacterInventory
    {
        public class Helm : ItemMesh
        {
            public Helm(IMeshLoader loader, string meshPath, string texturePath) : base(loader, meshPath,
                texturePath, MeshType.Helm, 8)
            {
            }
        }
    }
}