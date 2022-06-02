namespace CharacterEditor.Services
{
    public class CharacterManageService : ICharacterManageService
    {
        public Character CurrentCharacter { get; private set; }

        public void SelectCharacter(Character character)
        {
            CurrentCharacter = character;
        }
    }
}