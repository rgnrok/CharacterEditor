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
            var gameFactory = new CreateGameFactory(_loaderService);
            _gameFactory = gameFactory;
            _registerService.Register<ICreateGameFactory>(gameFactory);
            _registerService.Register<IMeshInstanceCreator>(gameFactory);
        }

        public void Exit()
        {
        }

        private async void OnSceneLoaded()
        {
            _loadingCurtain.SetLoading(10);

            var configs = await _loaderService.ConfigLoader.LoadConfigs();
            await ParseConfigs(configs);
            _loadingCurtain.SetLoading(50);


            await SetupServices();
            _fsm.SpawnEvent((int)GameStateMachine.GameStateType.CreateCharacterLoop);
        }

        private async Task ParseConfigs(CharacterConfig[] configs)
        {
            var data = new List<CharacterGameObjectData>(configs.Length);

            foreach (var config in configs)
            {
                var configData = await _gameFactory.SpawnCreateCharacter(config);
                if (configData == null) continue;

                data.Add(configData);
            }

            await _configManager.Init(data.ToArray());
        }


        private async Task SetupServices()
        {
            _loadingCurtain.SetLoading(10);

            if (TextureManager.Instance != null)
            {
                while (!TextureManager.Instance.IsReady)
                    await Task.Yield();
                _loadingCurtain.SetLoading(30);
            }

            if (MeshManager.Instance != null)
            {
                while (!MeshManager.Instance.IsReady)
                    await Task.Yield();
                _loadingCurtain.SetLoading(70);
            }

            _loadingCurtain.SetLoading(100);
        }
    }
}