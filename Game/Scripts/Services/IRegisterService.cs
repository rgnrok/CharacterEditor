using CharacterEditor;

namespace CharacterEditor.Services
{
    public interface IRegisterService : IService
    {
        void Register<TService>(TService service) where TService : IService;
    }
}