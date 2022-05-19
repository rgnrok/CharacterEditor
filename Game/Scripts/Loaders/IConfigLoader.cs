using System.Threading.Tasks;

namespace CharacterEditor
{
    public interface IConfigLoader : ICleanable
    {
        Task<CharacterConfig[]> LoadConfigs();
        Task<CharacterConfig> LoadConfig(string guid);
    }
}
