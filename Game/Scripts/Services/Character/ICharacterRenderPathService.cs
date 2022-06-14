using System;

namespace CharacterEditor.Services
{
    public interface ICharacterRenderPathService : IService
    {
        void FireStartDrawPath(Character character);

        event Action<Character> OnStartDrawPath;
    }
}