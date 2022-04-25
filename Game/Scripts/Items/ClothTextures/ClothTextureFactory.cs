namespace CharacterEditor
{
    namespace CharacterInventory
    {
        public class ClothTextureFactory
        {
            public static ItemTexture Create(TextureType type, ITextureLoader loader, string path)
            {
                switch (type)
                {
                    case TextureType.Torso:
                        return new ClothTextures.Torso(loader, path);
                    case TextureType.Pants:
                        return new ClothTextures.Pants(loader, path);
                    case TextureType.Head:
                        return new ClothTextures.Head(loader, path);
                    case TextureType.Glove:
                        return new ClothTextures.Gloves(loader, path);
                    case TextureType.Shoe:
                        return new ClothTextures.Shoe(loader, path);
                    case TextureType.Belt:
                        return new ClothTextures.Belt(loader, path);
                    case TextureType.RobeShort:
                    case TextureType.RobeLong:
                        return new ClothTextures.Robe(loader, path);
                    case TextureType.Cloak:
                        return new ClothTextures.Cloak(loader, path);
                }

                return null;
            }
        }
    }
}