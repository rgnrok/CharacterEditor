namespace CharacterEditor
{
    namespace Textures
    {
        public class Hair : AbstractTexture
        {
            public Hair(ITextureLoader loader, string characterRace) :
                base(loader, characterRace, TextureType.Hair) {
            }
        }
    }
}
