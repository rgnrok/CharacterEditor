using System.Collections.Generic;
using System.Threading.Tasks;
using CharacterEditor;
using CharacterEditor.Services;

namespace Game
{
    public class LoadCreateCharacterState : LoadState
    {
        private readonly IGameFactory _gameFactory;
        private readonly ConfigManager _configManager;

        public LoadCreateCharacterState(FSM fsm, SceneLoader sceneLoader, LoadingCurtain loadingCurtain, ILoaderService loaderService, IGameFactory gameFactory, ConfigManager configManager) : base(fsm, sceneLoader, loadingCurtain, loaderService)
        {
            _gameFactory = gameFactory;
            _configManager = configManager;
        }

        protected override async void OnLoaded()
        {
            var configs = await _loaderService.ConfigLoader.LoadConfigs();
            await ParseConfigs(configs);
            _loadingCurtain.SetLoading(50);


            await SetupServices();
            _fsm.SpawnEvent((int)GameStateMachine.GameStateType.CreateCharacter);
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