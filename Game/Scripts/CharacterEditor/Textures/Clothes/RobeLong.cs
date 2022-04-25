namespace CharacterEditor
{
    namespace Textures
    {
        public class RobeLong : AbstractTexture
        {

            public RobeLong(ITextureLoader loader, string characterRace) :
                base(loader, characterRace, TextureType.RobeLong) {
            }

            public override void MoveNext() {
                if (SelectedTexture == _textures.Length - 1 && _textures.Length > 1) {
                    SelectedTexture = 1; //Skip empty robe
                }
                else {
                    base.MoveNext();
                }
            }

            public override void MovePrev() {
                if (SelectedTexture == 1 && _textures.Length > 1) {
                    SelectedTexture = _textures.Length - 1; //Skip empty robe
                }
                else {
                    base.MovePrev();
                }
            }
        }
    }
}
