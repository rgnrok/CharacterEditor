namespace CharacterEditor
{
    namespace Textures
    {
        public class Skin : AbstractTexture
        {
            public Skin(ITextureLoader loader, string characterRace) :
                base(loader, characterRace, TextureType.Skin)
            {
            }
        }
    }
}

