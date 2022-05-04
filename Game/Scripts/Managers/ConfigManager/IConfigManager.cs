using System;
using System.Threading.Tasks;

namespace CharacterEditor
{
    public interface IConfigManager : IService
    {
        CharacterConfig Config { get; }
        CharacterGameObjectData ConfigData { get; }
        Task OnNextCharacter();
        Task OnPrevCharacter();

        Task Init(CharacterGameObjectData[] configData);

        event Action OnChangeCharacter;
        event Func<CharacterGameObjectData, Task> OnChangeConfig;
    }
}