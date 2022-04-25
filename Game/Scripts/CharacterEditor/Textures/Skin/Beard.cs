namespace CharacterEditor
{
    namespace Textures
    {
        public class Beard : AbstractTexture
        {
            public Beard(ITextureLoader loader, string characterRace) :
                base(loader, characterRace, TextureType.Beard) {
            }
        }
    }
}
