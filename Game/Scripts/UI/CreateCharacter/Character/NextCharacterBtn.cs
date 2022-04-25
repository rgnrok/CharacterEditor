namespace CharacterEditor
{
    public class NextCharacterBtn : ChangeCharacterBtn
    {
        protected override CharacterChangeType _type
        {
            get { return CharacterChangeType.Forward; }
        }
    }
}
