using System.Collections.Generic;
using CharacterEditor;

namespace Assets.Game.Scripts.Loaders
{
    public interface IDataManager
    {
        string[][] ParseTextures(string characterRace, TextureType type);
        Dictionary<string, string[][]> ParseMeshes(string characterRace, MeshType meshType);

    }
}