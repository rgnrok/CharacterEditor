using System.Collections.Generic;
using System.Threading.Tasks;
using CharacterEditor;
using CharacterEditor.Services;

namespace Game
{
    public class LoadGameState : IPayloadedState<string>
    {
        private const string PLAY_CHARACTER_SCENE = "Play_Character_Scene";

        private readonly FSM _fsm;
        private readonly ILoaderService _loaderService;
        private readonly SceneLoader _sceneLoader;
        private readonly LoadingCurtain _loadingCurtain;
        private readonly IGameFactory _gameFactory;
        private readonly IConfigManager _configManager;
        private readonly ISaveLoadService _saveLoadService;
        private string _saveName;


        public LoadGameState(FSM fsm, SceneLoader sceneLoader, LoadingCurtain loadingCurtain, ILoaderService loaderService, IGameFactory gameFactory, IConfigManager configManager, ISaveLoadService saveLoadService) 
        {
            _fsm = fsm;
            _sceneLoader = sceneLoader;
            _loadingCurtain = loadingCurtain;
            _loaderService = loaderService;
            _gameFactory = gameFactory;
            _configManager = configManager;
            _saveLoadService = saveLoadService;
        }

        public void Enter(string saveName)
        {
            _saveName = saveName;
            _loadingCurtain.SetLoading(0);
            _sceneLoader.Load(PLAY_CHARACTER_SCENE, OnSceneLoaded);
        }

        public void Exit()
        {
        }

        private async void OnSceneLoaded()
        {
            await _loaderService.Initialize();
            _loadingCurtain.SetLoading(10);
            
            var configs = await _loaderService.ConfigLoader.LoadConfigs();
            await ParseConfigs(configs);
            _loadingCurtain.SetLoading(20);

            var successLoad = await _saveLoadService.Load(_saveName, (loadPercent) =>
            {
                _loadingCurtain.SetLoading(20 + loadPercent / 2.5f); //20-60%

            });
            if (!successLoad)
                _fsm.SpawnEvent((int)GameStateMachine.GameStateType.CreateGame);
            else
                _fsm.SpawnEvent((int) GameStateMachine.GameStateType.GameLoop);
        }

        private async Task ParseConfigs(CharacterConfig[] configs)
        {
            var data = new List<CharacterGameObjectData>(configs.Length);

            foreach (var config in configs)
            {
                var configData = await _gameFactory.SpawnGameCharacter(config);
                if (configData == null) continue;

                data.Add(configData);
            }

            await _configManager.Init(data.ToArray());
        }
    }
}