namespace CharacterEditor
{
    namespace Textures
    {
        public class RobeShort : CharacterTexture
        {
            public RobeShort(ITextureLoader loader, string[][] textures) :
                base(loader, textures, TextureType.RobeShort) {
            }

            public override void MoveNext() {
                if (SelectedTextureIndex == _textures.Length - 1 && _textures.Length > 1) {
                    SelectedTextureIndex = 1; //Skip empty robe
                }
                else {
                    SelectedTextureIndex++;
                }
            }

            public override void MovePrev() {
                if (SelectedTextureIndex == 1 && _textures.Length > 1) {
                    SelectedTextureIndex = _textures.Length - 1; //Skip empty robe
                }
                else {
                    SelectedTextureIndex--;
                }
            }
        }
    }
}
