namespace CharacterEditor
{
    namespace CharacterInventory
    {
        public class Arm : ItemMesh
        {
            public Arm(IMeshLoader loader, string meshPath, string texturePath, MeshType type) : base(loader, meshPath,
                texturePath, type, type == MeshType.ArmLeft ? 6 : 7)
            {
            }
        }
    }
}