using System;
using System.Threading.Tasks;

namespace CharacterEditor
{
    /*
     * Loading and parsing character configs
     */
    public interface IConfigLoader : IService
    {
        Task<CharacterConfig[]> LoadConfigs();
        Task<CharacterConfig> LoadConfig(string guid);
    }
}
