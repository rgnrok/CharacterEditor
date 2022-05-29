using CharacterEditor.CharacterInventory;

namespace CharacterEditor
{
    public class FaceMeshFactory
    {
        public static FaceMesh Create(MeshType type, string meshPath, IMeshLoader loader)
        {
            switch (type)
            {
                case MeshType.Beard:
                case MeshType.Hair:
                case MeshType.FaceFeature:
                    var itemMesh = new ItemMesh(type, loader, meshPath);
                    return new FaceMesh(itemMesh);
            }

            return null;
        }
    }
}