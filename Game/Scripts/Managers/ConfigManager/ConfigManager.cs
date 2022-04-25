using System;
using System.Threading.Tasks;


namespace CharacterEditor
{
    public class ConfigManager : IService
    {
        public CharacterConfig Config => 
            ConfigData.Config;

        public CharacterGameObjectData ConfigData => 
            _charactersGameObjectData[CurrentConfigIndex];

        private int _currentConfigIndex;
        private int CurrentConfigIndex
        {
            get => _currentConfigIndex;
            set
            {
                if (value < 0)
                    value = _charactersGameObjectData.Length - 1;
                
                if (value >= _charactersGameObjectData.Length)
                    value = 0;
                
                _currentConfigIndex = value;
            }
        }

        private CharacterGameObjectData[] _charactersGameObjectData;

        public Action OnChangeCharacter;


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
            await TextureManager.Instance.ApplyConfig(Config, ConfigData);
            await MeshManager.Instance.ApplyConfig(Config, ConfigData);

           

            ConfigData.CharacterObject.SetActive(true);

            OnChangeCharacter?.Invoke();
        }
    }
}