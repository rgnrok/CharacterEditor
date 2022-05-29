namespace CharacterEditor
{
    namespace Textures
    {
        public class RobeLong : CharacterTexture
        {

            public RobeLong(ITextureLoader loader, string[][] textures) :
                base(loader, textures, TextureType.RobeLong) {
            }

            public override void MoveNext() {
                if (SelectedTextureIndex == _textures.Length - 1 && _textures.Length > 1) {
                    SelectedTextureIndex = 1; //Skip empty robe
                }
                else {
                    base.MoveNext();
                }
            }

            public override void MovePrev() {
                if (SelectedTextureIndex == 1 && _textures.Length > 1) {
                    SelectedTextureIndex = _textures.Length - 1; //Skip empty robe
                }
                else {
                    base.MovePrev();
                }
            }
        }
    }
}
