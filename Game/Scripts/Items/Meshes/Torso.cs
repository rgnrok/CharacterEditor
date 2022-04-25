namespace CharacterEditor
{
    namespace CharacterInventory
    {
        public class Torso : ItemMesh
        {
            public Torso(IMeshLoader loader, string meshPath, string texturePath) : base(loader, meshPath,
                texturePath, MeshType.Torso, 2)
            {
            }
        }
    }
}