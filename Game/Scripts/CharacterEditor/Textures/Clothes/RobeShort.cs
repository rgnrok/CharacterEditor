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
                if (SelectedTexture == _textures.Length - 1 && _textures.Length > 1) {
                    SelectedTexture = 1; //Skip empty robe
                }
                else {
                    SelectedTexture++;
                }
            }

            public override void MovePrev() {
                if (SelectedTexture == 1 && _textures.Length > 1) {
                    SelectedTexture = _textures.Length - 1; //Skip empty robe
                }
                else {
                    SelectedTexture--;
                }
            }
        }
    }
}
