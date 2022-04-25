using System.Collections.Generic;
using System.Threading.Tasks;
using CharacterEditor;
using CharacterEditor.CharacterInventory;
using CharacterEditor.Services;
using UnityEngine;

namespace Game
{
    public class LoadGameState : LoadState
    {
        private readonly IGameFactory _gameFactory;
        private readonly ConfigManager _configManager;

        public LoadGameState(FSM fsm, SceneLoader sceneLoader, LoadingCurtain loadingCurtain, ILoaderService loaderService, IGameFactory gameFactory, ConfigManager configManager) : base(fsm, sceneLoader, loadingCurtain, loaderService)
        {
            _gameFactory = gameFactory;
            _configManager = configManager;
        }

        protected override async void OnLoaded()
        {
            var configs = await _loaderService.ConfigLoader.LoadConfigs();
            await ParseConfigs(configs);
            _loadingCurtain.SetLoading(50);


            await LoadingCoroutine();
            _fsm.SpawnEvent((int)GameStateMachine.GameStateType.CreateCharacter);
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


        private async Task LoadingBaseComponentsHandlerCompleted()
        {
            var saveName = PlayerPrefs.GetString(SaveManager.LOADED_SAVE_KEY);
            if (saveName == null)
            {

                var saves = SaveManager.Instance.GetSaves();
                if (saves.Length == 0)
                {
                    _fsm.SpawnEvent((int) GameStateMachine.GameStateType.PlayLoop);
                    return;
                }

                saveName = saves[0];
            }

            await SaveManager.Instance.Load(saveName, progress =>
            {
                _loadingCurtain.SetLoading(progress);
            });
        }

        private async Task LoadingCoroutine()
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


            if (ItemManager.Instance != null)
            {
                //                while (!ItemManager.Instance.IsReady)
                //                    yield return null;
                _loadingCurtain.SetLoading(90);
                await LoadingBaseComponentsHandlerCompleted();
            }
            else
            {
                _loadingCurtain.SetLoading(100);
            }
        }
    }
}