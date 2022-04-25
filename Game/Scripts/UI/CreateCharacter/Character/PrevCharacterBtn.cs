namespace CharacterEditor
{
    public class PrevCharacterBtn : ChangeCharacterBtn
    {
        protected override CharacterChangeType _type
        {
            get { return CharacterChangeType.Back; }
        }
    }
}

