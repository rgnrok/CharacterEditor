namespace CharacterEditor
{
    namespace Textures
    {
        public class Cloak : AbstractTexture
        {
            public Cloak(ITextureLoader loader, string characterRace) :
                base(loader, characterRace, TextureType.Cloak) {
            }
        }
    }
}
