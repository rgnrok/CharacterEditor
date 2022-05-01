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
            RegisterStaticDataService();

            _services.RegisterSingle<IFSM>(_fsm);
            _services.RegisterSingle<ICoroutineRunner>(_coroutineRunner);
            _services.RegisterSingle<ILoaderService>(new LoaderService(_services.Single<IStaticDataService>(), _coroutineRunner));
            _services.RegisterSingle<IGameFactory>(new GameFactory(_services.Single<ILoaderService>()));

            _services.RegisterSingle<IConfigManager>(new ConfigManager());
            _services.RegisterSingle<ISaveLoadService>(new SaveLoadService(_services.Single<ILoaderService>(), _services.Single<IGameFactory>(), _coroutineRunner));
        }

        private void RegisterStaticDataService()
        {
            var staticData = new StaticDataService();
            staticData.LoadData();

            _services.RegisterSingle<IStaticDataService>(staticData);
        }
    }
}