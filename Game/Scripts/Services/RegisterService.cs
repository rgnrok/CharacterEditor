namespace CharacterEditor.Services
{
    public class RegisterService : IRegisterService
    {
        private readonly AllServices _services;

        public RegisterService(AllServices services)
        {
            _services = services;
        }

        public void Register<TService>(TService service) where TService : IService
        {
            _services.RegisterSingle<TService>(service);
        }
    }
}