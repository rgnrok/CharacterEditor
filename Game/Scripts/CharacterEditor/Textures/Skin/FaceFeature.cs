namespace CharacterEditor
{
    namespace Textures
    {
        public class FaceFeature : AbstractTexture
        {
            public FaceFeature(ITextureLoader loader, string characterRace) :
                base(loader, characterRace, TextureType.FaceFeature) {
            }
        }
    }
}
