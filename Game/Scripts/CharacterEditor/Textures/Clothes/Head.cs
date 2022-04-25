namespace CharacterEditor
{
    namespace Textures
    {
        public class Head : AbstractTexture
        {
            public Head(ITextureLoader loader, string characterRace) :
                base(loader, characterRace, TextureType.Head) {
            }
        }
    }
}

