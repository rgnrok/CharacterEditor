namespace CharacterEditor
{
    namespace Textures
    {
        public class Gloves : AbstractTexture
        {
            public Gloves(ITextureLoader loader, string characterRace) :
                base(loader, characterRace, TextureType.Glove) {
            }
        }
    }
}
