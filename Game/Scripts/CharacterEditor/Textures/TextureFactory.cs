namespace CharacterEditor
{
    public class TextureFactory
    {
        public static AbstractTexture Create(TextureType type, ITextureLoader loader, string characterRace)
        {
            switch (type)
            {
                case TextureType.Skin:
                    // Pos 0, 0; Size 1024, 1024,
                    return new Textures.Skin(loader, characterRace);
                case TextureType.Eyebrow:
                    // Pos 384, 0; Size 640, 384
                    return new Textures.Eyebrow(loader, characterRace);
                case TextureType.Scar:
                    // Pos 384, 0; Size 640, 384
                    return new Textures.Scar(loader,  characterRace);
                case TextureType.Beard:
                    // Pos 384, 0; Size 640, 384
                    return new Textures.Beard(loader, characterRace);
                case TextureType.FaceFeature:
                    // Pos 384, 0; Size 640, 384
                    return new Textures.FaceFeature(loader, characterRace);
                case TextureType.Hair:
                    // Pos 384, 0; Size 640, 384
                    return new Textures.Hair(loader, characterRace);
                case TextureType.Eye:
                    // Pos 960, 0; Size 64, 64
                    return new Textures.Eye(loader, characterRace);
                case TextureType.Head:
                    // Pos 384, 0; Size 640, 384
                    return new Textures.Head(loader, characterRace);
                case TextureType.Pants:
                    // Pos 0, 0; Size 512, 576
                    return new Textures.Pants(loader, characterRace);
                case TextureType.Torso:
                    // Pos 0, 512; Size 1024, 512,
                    return new Textures.Torso(loader, characterRace);
                case TextureType.Shoe:
                    // Pos 0, 0; Size 384, 448,
                    return new Textures.Shoes(loader, characterRace);
                case TextureType.Glove:
                    // Pos 512, 384; Size 512, 448
                    return new Textures.Gloves(loader, characterRace);
                case TextureType.RobeLong:
                    // Pos 0, 0; Size 512, 576
                    return new Textures.RobeLong(loader, characterRace);
                case TextureType.RobeShort:
                    // Pos 0, 0; Size 512, 576
                    return new Textures.RobeShort(loader,characterRace);
                case TextureType.Belt:
                    // Pos 0, 512; Size 512, 256
                    return new Textures.Belt(loader, characterRace);
                case TextureType.Cloak:
                    // Pos 0, 0; Size 512, 512
                    return new Textures.Cloak(loader, characterRace);
            }

            return null;
        }
    }
}