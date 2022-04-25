namespace CharacterEditor
{
    namespace Textures
    {
        public class Torso : AbstractTexture
        {
            public Torso(ITextureLoader loader, string characterRace) :
                base(loader, characterRace, TextureType.Torso) {
            }
        }
    }
}
