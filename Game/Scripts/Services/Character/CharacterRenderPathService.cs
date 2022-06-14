using System;

namespace CharacterEditor.Services
{
    class CharacterRenderPathService : ICharacterRenderPathService
    {
        public event Action<Character> OnStartDrawPath;

        public void FireStartDrawPath(Character character)
        {
            OnStartDrawPath?.Invoke(character);
        }
    }
}