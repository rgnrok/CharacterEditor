using CharacterEditor;
using CharacterEditor.Services;

namespace Game
{
    public class BootstrapState : IState
    {
        private readonly FSM _fsm;
        private readonly ICoroutineRunner _coroutineRunner;
        private readonly AllServices _services;

        public BootstrapState(FSM fsm, ICoroutineRunner coroutineRunner, AllServices services)
        {
            _fsm = fsm;
            _coroutineRunner = coroutineRunner;
            _services = services;

            RegisterServices();
        }

        public void Enter()
        {
            _fsm.SpawnEvent((int) GameStateMachine.GameStateType.Load, "Create_Character_Scene");
        }

        public void Exit()
        {
        }

        private void RegisterServices()
        {
            RegisterStaticDataService();
            RegisterAssetProvider();

            _services.RegisterSingle<ICoroutineRunner>(_coroutineRunner);
            _services.RegisterSingle<ILoaderService>(new LoaderService(_services.Single<IStaticDataService>(), _coroutineRunner));
            _services.RegisterSingle<IGameFactory>(new GameFactory(_services.Single<IAssets>()));

            _services.RegisterSingle<ConfigManager>(new ConfigManager());
        }

        private void RegisterStaticDataService()
        {
            var staticData = new StaticDataService();
            staticData.LoadData();

            _services.RegisterSingle<IStaticDataService>(staticData);
        }

        private void RegisterAssetProvider()
        {
            var assetsProvider = new AssetsProvider();
            assetsProvider.InitializeAsync();
            _services.RegisterSingle<IAssets>(assetsProvider);
        }
    }
}