namespace CharacterEditor
{
    namespace CharacterInventory
    {
        public class Shoulder : ItemMesh
        {
            public Shoulder(IMeshLoader loader, string meshPath, string texturePath, MeshType type) : base(loader, meshPath,
                texturePath, type, type == MeshType.ShoulderLeft ? 4 : 5)
            {
            }
        }
    }
}