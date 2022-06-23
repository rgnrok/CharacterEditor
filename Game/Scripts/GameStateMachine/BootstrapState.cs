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

            var loaderService = new LoaderService(_services.Single<IStaticDataService>(), _coroutineRunner);
            _services.RegisterSingle<ILoaderService>(loaderService);

            _services.RegisterSingle<IInputService>(new InputService(loaderService.CursorLoader));

            _services.RegisterSingle<IConfigManager>(new ConfigManager());
            _services.RegisterSingle<ISaveLoadStorage>(new FileSaveLoadStorage());

            _services.RegisterSingle<ICharacterEquipItemService>(
                new CharacterEquipItemService(_services.Single<IMergeTextureService>(), loaderService));

            _services.RegisterSingle<IGameFactory>(new GameFactory(loaderService,
                _services.Single<ICharacterEquipItemService>()));

            _services.RegisterSingle<ISaveLoadService>(new SaveLoadService(_services.Single<ISaveLoadStorage>(),
                loaderService, _services.Single<IGameFactory>()));

            _services.RegisterSingle<ICharacterMoveService>(new CharacterMoveService());
            _services.RegisterSingle<ICharacterRenderPathService>(new CharacterRenderPathService());
            _services.RegisterSingle<ICharacterManageService>(new CharacterManageService());

            _services.RegisterSingle<IBattleManageService>(new BattleManager(_coroutineRunner));
            _services.RegisterSingle<ICharacterPathCalculation>(new CharacterNavMeshPathCalculation());
        }

        private void RegisterStaticDataService()
        {
            var staticData = new StaticDataService();
            staticData.LoadData();

            _services.RegisterSingle<IStaticDataService>(staticData);
        }
    }
}