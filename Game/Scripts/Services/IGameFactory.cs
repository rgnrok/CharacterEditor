using System.Threading.Tasks;

namespace CharacterEditor.Services
{
    public interface IGameFactory : IService
    {
        Task<CharacterGameObjectData> SpawnCreateCharacter(CharacterConfig config);
        Task<CharacterGameObjectData> SpawnGameCharacter(CharacterConfig config);
    }
}