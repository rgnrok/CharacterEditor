namespace CharacterEditor
{
    namespace CharacterInventory
    {
        namespace ClothTextures
        {
            public class Gloves : ItemTexture
            {
                public Gloves(ITextureLoader loader, string path) : base(loader, path, TextureType.Glove)
                {
                }

                public override string GetShaderTextureName()
                {
                    return "_GloveTex";
                }
            }
        }
    }
}