namespace CharacterEditor
{
    namespace CharacterInventory
    {
        public class ClothTextureFactory
        {
            public static ItemTexture Create(TextureType type, ITextureLoader loader, string path)
            {
                return new ItemTexture(loader, path, type);
            }
        }
    }
}