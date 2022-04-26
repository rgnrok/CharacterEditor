using Assets.Game.Scripts.Loaders;

namespace CharacterEditor
{
    public class TextureFactory
    {
        public static CharacterTexture Create(TextureType type, ITextureLoader loader, IDataManager dataManager, string characterRace)
        {
            var textures = dataManager.ParseTextures(characterRace, type);
            switch (type)
            {
                case TextureType.RobeLong:
                    return new Textures.RobeLong(loader, textures);
                case TextureType.RobeShort:
                    return new Textures.RobeShort(loader, textures);
                default:
                    return new CharacterTexture(loader, textures, type);
            }
        }
    }
}