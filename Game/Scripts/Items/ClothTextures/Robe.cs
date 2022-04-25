namespace CharacterEditor
{
    namespace CharacterInventory
    {
        namespace ClothTextures
        {
            public class Robe : ItemTexture
            {
                public Robe(ITextureLoader loader, string path) : base(loader, path, TextureType.RobeShort)
                {
                }

                public override string GetShaderTextureName()
                {
                    return "_RobeTex";
                }
            }
        }
    }
}