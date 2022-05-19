namespace CharacterEditor
{
    public class NextCharacterBtn : ChangeCharacterBtn
    {
        protected override CharacterChangeType Type => 
            CharacterChangeType.Forward;
    }
}
