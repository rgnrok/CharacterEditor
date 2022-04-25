namespace CharacterEditor
{
    namespace CharacterInventory
    {
        namespace ClothTextures
        {
            public class Pants : ItemTexture
            {
                public Pants(ITextureLoader loader, string path) : base(loader, path, TextureType.Pants)
                {
                }

                public override string GetShaderTextureName()
                {
                    return "_PantsTex";
                }
            }
        }
    }
}