namespace CharacterEditor
{
    namespace CharacterInventory
    {
        namespace ClothTextures
        {
            public class Shoe : ItemTexture
            {
                public Shoe(ITextureLoader loader, string path) : base(loader, path, TextureType.Shoe)
                {
                }

                public override string GetShaderTextureName()
                {
                    return "_ShoeTex";
                }
            }
        }
    }
}