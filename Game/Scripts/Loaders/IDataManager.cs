using System.Collections.Generic;

namespace CharacterEditor
{
    public interface IDataManager
    {
        string[][] ParseCharacterTextures(string characterRace, TextureType type);
        Dictionary<string, string[][]> ParseCharacterMeshes(string characterRace, MeshType meshType);

    }
}