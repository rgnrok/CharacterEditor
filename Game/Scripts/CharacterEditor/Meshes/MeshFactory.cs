using System;
using CharacterEditor.Mesh;

namespace CharacterEditor
{
    public class MeshFactory
    {
        public static CharacterMesh Create(IMeshLoader loader, ITextureLoader textureLoader, IDataManager dataManager, MeshType meshType, CharacterConfig characterConfig)
        {
            var meshTexturesPath = dataManager.ParseCharacterMeshes(characterConfig, meshType);
            switch (meshType)
            {
                case MeshType.Beard:
                case MeshType.FaceFeature:
                case MeshType.Hair:
                    return new CharacterMesh(loader, textureLoader, meshTexturesPath, meshType, true);
                default:
                    return new CharacterMesh(loader, textureLoader, meshTexturesPath, meshType, false);
            }
        }
    }
}
