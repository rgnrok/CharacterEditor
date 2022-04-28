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

        event Action OnChangeCharacter;
        Task Init(CharacterGameObjectData[] configData);
    }
}