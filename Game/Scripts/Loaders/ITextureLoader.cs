using UnityEngine;

namespace CharacterEditor
{
    public interface ITextureLoader : ICommonLoader<Texture2D>, IService
    {
        string[][] ParseCharacterTextures(string characterRace, TextureType type);
    }
}
