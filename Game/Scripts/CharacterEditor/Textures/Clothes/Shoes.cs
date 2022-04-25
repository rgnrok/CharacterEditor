namespace CharacterEditor
{
    namespace Textures
    {
        public class Shoes : AbstractTexture
        {
            public Shoes(ITextureLoader loader, string characterRace) :
                base(loader, characterRace, TextureType.Shoe) {
            }
        }
    }
}
