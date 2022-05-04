using System;
using System.Linq;
using System.Threading.Tasks;


namespace CharacterEditor
{
    public class ConfigManager : IConfigManager
    {
        private int _currentConfigIndex;
        private int CurrentConfigIndex
        {
            get => _currentConfigIndex;
            set => 
                _currentConfigIndex = Helper.GetActualIndex(value, _charactersGameObjectData.Length);
        }

        public CharacterConfig Config =>
            ConfigData.Config;

        public CharacterGameObjectData ConfigData =>
            _charactersGameObjectData[CurrentConfigIndex];

        private CharacterGameObjectData[] _charactersGameObjectData;

        public event Action OnChangeCharacter;
        public event Func<CharacterGameObjectData, Task> OnChangeConfig;


        public async Task Init(CharacterGameObjectData[] configData)
        {
            _charactersGameObjectData = configData;
            await ChangeCharacter();
        }


        public async Task OnNextCharacter()
        {
            if (_charactersGameObjectData.Length == 1 && ConfigData.CharacterObject.activeSelf)  return;

            ConfigData.CharacterObject.SetActive(false);
            CurrentConfigIndex++;

            await ChangeCharacter();
        }

        public async Task OnPrevCharacter()
        {
            if (_charactersGameObjectData.Length == 1 && ConfigData.CharacterObject.activeSelf)
                return;

            ConfigData.CharacterObject.SetActive(false);
            CurrentConfigIndex--;

            await ChangeCharacter();
        }

        private async Task ChangeCharacter()
        {
            if (OnChangeConfig != null)
            {
                await Task.WhenAll(OnChangeConfig.GetInvocationList()
                    .Select(eDelegate => ((Func<CharacterGameObjectData, Task>)eDelegate)(ConfigData)));
            }
            ConfigData.CharacterObject.SetActive(true);

            OnChangeCharacter?.Invoke();
        }
    }
}