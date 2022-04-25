namespace CharacterEditor
{
    namespace CharacterInventory
    {
        namespace ClothTextures
        {
            public class Head : ItemTexture
            {
                public Head(ITextureLoader loader, string path) : base(loader, path, TextureType.Head)
                {
                }

                public override string GetShaderTextureName()
                {
                    return "_HeadTex";
                }
            }
        }
    }
}