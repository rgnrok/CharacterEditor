using System.Threading.Tasks;

namespace CharacterEditor.Services
{
    public interface ICreateGameFactory : IService
    {
        Task<CharacterGameObjectData> SpawnCreateCharacter(CharacterConfig config);
    }
}