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
                case TextureType.Belt:
                case TextureType.Cloak:
                case TextureType.Glove:
                case TextureType.Head:
                case TextureType.Shoe:

                case TextureType.FaceFeature:
                case TextureType.Hair:
                case TextureType.Scar:
                case TextureType.Beard:
                    return new CharacterTexture(loader, textures, type, hasEmptyTexture: true);
                default:
                    return new CharacterTexture(loader, textures, type);
            }
        }
    }
}