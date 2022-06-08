using System.Collections.Generic;

namespace CharacterEditor
{
    public interface IDataManager
    {
        string[][] ParseCharacterTextures(CharacterConfig characterConfig, TextureType type);
        Dictionary<string, string[][]> ParseCharacterMeshes(CharacterConfig characterConfig, MeshType meshType);
        Dictionary<string, string> ParseGameMaterials();
    }
}