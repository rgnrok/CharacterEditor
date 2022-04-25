namespace CharacterEditor
{
    namespace Textures
    {
        public class Pants : AbstractTexture
        {
            public Pants(ITextureLoader loader, string characterRace) :
                base(loader, characterRace, TextureType.Pants) {
            }
        }
    }
}
