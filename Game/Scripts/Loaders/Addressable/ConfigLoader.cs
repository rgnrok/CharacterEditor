using System.Threading.Tasks;

namespace CharacterEditor
{
    namespace AddressableLoader
    {
        public class ConfigLoader : CommonLoader<CharacterConfig>, IConfigLoader
        {
            private readonly RemoteDataManager _dataManager;

            public ConfigLoader(RemoteDataManager dataManager)
            {
                _dataManager = dataManager;
            }

            public async Task<CharacterConfig[]> LoadConfigs()
            {
                var configs = new CharacterConfig[_dataManager.Races.Values.Count];
                var i = 0;
                foreach (var configInfo in _dataManager.Races.Values)
                    configs[i++] = await LoadByPath(configInfo.configPath);

                return configs;
            }

            public async Task<CharacterConfig> LoadConfig(string guid)
            {
                if (!_dataManager.Races.TryGetValue(guid, out var raceMap))
                    return null;

                return await LoadByPath(raceMap.configPath);
            }
        }
    }
}