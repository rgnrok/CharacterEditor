namespace CharacterEditor
{
    namespace CharacterInventory
    {
        namespace ClothTextures
        {
            public class Belt : ItemTexture
            {
                public Belt(ITextureLoader loader, string path) : base(loader, path, TextureType.Belt)
                {
                }

                public override string GetShaderTextureName()
                {
                    return "_BeltTex";
                }
            }
        }
    }
}