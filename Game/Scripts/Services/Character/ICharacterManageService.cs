namespace CharacterEditor.Services
{
    public interface ICharacterManageService : IService
    {
        Character CurrentCharacter { get; }
        void SelectCharacter(Character character);
    }
}