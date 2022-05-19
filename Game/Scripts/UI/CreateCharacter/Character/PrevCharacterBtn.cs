namespace CharacterEditor
{
    public class PrevCharacterBtn : ChangeCharacterBtn
    {
        protected override CharacterChangeType Type => 
            CharacterChangeType.Back;
    }
}

