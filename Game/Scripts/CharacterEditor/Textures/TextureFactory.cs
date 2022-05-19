namespace CharacterEditor
{
    public class TextureFactory
    {
        public static CharacterTexture Create(TextureType type, ITextureLoader loader, IDataManager dataManager, CharacterConfig characterConfig)
        {
            var textures = dataManager.ParseCharacterTextures(characterConfig, type);
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