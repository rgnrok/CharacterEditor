using System.Collections.Generic;
using System.Threading.Tasks;
using CharacterEditor;
using CharacterEditor.Services;

namespace Game
{
    public class CreateGameState : IState
    {
        private const string CREATE_CHARACTER_SCENE = "Create_Character_Scene";
        private readonly FSM _fsm;
        private readonly ILoaderService _loaderService;
        private readonly SceneLoader _sceneLoader;
        private readonly LoadingCurtain _loadingCurtain;
        private readonly IConfigManager _configManager;
        private readonly IRegisterService _registerService;

        private ICreateGameFactory _gameFactory;
        private bool _isRegistered;

        public CreateGameState(FSM fsm, SceneLoader sceneLoader, LoadingCurtain loadingCurtain,
            ILoaderService loaderService, IConfigManager configManager, IRegisterService registerService) 
        {
            _fsm = fsm;
            _sceneLoader = sceneLoader;
            _loadingCurtain = loadingCurtain;
            _loaderService = loaderService;
            _configManager = configManager;
            _registerService = registerService;
        }

        public async void Enter()
        {
            _loadingCurtain.SetLoading(0);
            await _loaderService.Initialize();

            RegisterServices();

            _sceneLoader.Load(CREATE_CHARACTER_SCENE, OnSceneLoaded);
        }

        private void RegisterServices()
        {
            if (_isRegistered) return;

            var gameFactory = new CreateGameFactory(_loaderService);
            _gameFactory = gameFactory;
            _registerService.Register<ICreateGameFactory>(gameFactory);
            _registerService.Register<IMeshInstanceCreator>(gameFactory);
            _isRegistered = true;
        }

        public void Exit()
        {
        }

        private async void OnSceneLoaded()
        {
            _loadingCurtain.SetLoading(10);

            var configs = await _loaderService.ConfigLoader.LoadConfigs();
            _loadingCurtain.SetLoading(20);

            await ParseConfigs(configs);
            _loadingCurtain.SetLoading(90);
            await AwaitManagers();
            _loadingCurtain.SetLoading(100);
            _fsm.SpawnEvent((int)GameStateMachine.GameStateType.CreateCharacterLoop);
        }

        private async Task ParseConfigs(CharacterConfig[] configs)
        {
            var configsCount = configs.Length;
            var data = new List<CharacterGameObjectData>(configsCount);

            for (var i = 0; i < configsCount; i++)
            {
                var configData = await _gameFactory.SpawnCreateCharacter(configs[i]);
                if (configData == null) continue;

                data.Add(configData);
                _loadingCurtain.SetLoading(20 + 50 / (configsCount - i)); 
            }

            await _configManager.Init(data.ToArray());
        }


        private async Task AwaitManagers()
        {
            if (TextureManager.Instance != null)
            {
                while (!TextureManager.Instance.IsReady)
                    await Task.Yield();
            }

            if (MeshManager.Instance != null)
            {
                while (!MeshManager.Instance.IsReady)
                    await Task.Yield();
            }
        }
    }
}