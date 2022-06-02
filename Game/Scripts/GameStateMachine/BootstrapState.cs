using CharacterEditor;
using CharacterEditor.Services;

namespace Game
{
    public class BootstrapState : IState
    {
        private readonly IFSM _fsm;
        private readonly ICoroutineRunner _coroutineRunner;
        private readonly AllServices _services;

        public BootstrapState(IFSM fsm, ICoroutineRunner coroutineRunner, AllServices services)
        {
            _fsm = fsm;
            _coroutineRunner = coroutineRunner;
            _services = services;

            RegisterServices();
        }

        public void Enter()
        {
            _fsm.SpawnEvent((int) GameStateMachine.GameStateType.LoadProgress);
        }

        public void Exit()
        {
        }

        private void RegisterServices()
        {
            _services.RegisterSingle<IRegisterService>(new RegisterService(_services));

            RegisterStaticDataService();

            _services.RegisterSingle<IFSM>(_fsm);
            _services.RegisterSingle<ICoroutineRunner>(_coroutineRunner);
            _services.RegisterSingle<IMergeTextureService>(new MergeTextureService());
            _services.RegisterSingle<ILoaderService>(new LoaderService(_services.Single<IStaticDataService>(), _coroutineRunner));

            _services.RegisterSingle<IInputService>(new InputService(_services.Single<ILoaderService>().CursorLoader));

            _services.RegisterSingle<IConfigManager>(new ConfigManager());
            _services.RegisterSingle<ISaveLoadStorage>(new FileSaveLoadStorage());
            _services.RegisterSingle<ISaveService>(new SaveService(_services.Single<ISaveLoadStorage>()));
        }

        private void RegisterStaticDataService()
        {
            var staticData = new StaticDataService();
            staticData.LoadData();

            _services.RegisterSingle<IStaticDataService>(staticData);
        }
    }
}