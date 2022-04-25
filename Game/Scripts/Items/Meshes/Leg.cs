namespace CharacterEditor
{
    namespace CharacterInventory
    {
        public class Leg : ItemMesh
        {
            public Leg(IMeshLoader loader, string meshPath, string texturePath, MeshType type) : base(loader, meshPath,
                texturePath, type, type == MeshType.LegLeft ? 11 : 12)
            {
            }
        }
    }
}