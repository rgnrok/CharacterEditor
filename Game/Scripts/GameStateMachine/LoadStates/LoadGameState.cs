using CharacterEditor;
using CharacterEditor.Services;
using CharacterEditor.StaticData;
using UnityEngine.SceneManagement;

namespace Game
{
    public class LoadGameState : IPayloadedState<string>
    {
        private const string PLAY_CHARACTER_SCENE = "Play_Character_Scene";

        private readonly FSM _fsm;
        private readonly ILoaderService _loaderService;
        private readonly SceneLoader _sceneLoader;
        private readonly LoadingCurtain _loadingCurtain;
        private readonly ISaveLoadService _saveLoadService;
        private readonly IStaticDataService _staticData;
        private string _saveName;

        private LevelStaticData LevelStaticData() =>
            _staticData.ForLevel(SceneManager.GetActiveScene().name);


        public LoadGameState(FSM fsm, SceneLoader sceneLoader, LoadingCurtain loadingCurtain, ILoaderService loaderService, ISaveLoadService saveLoadService, IStaticDataService staticData) 
        {
            _fsm = fsm;
            _sceneLoader = sceneLoader;
            _loadingCurtain = loadingCurtain;
            _loaderService = loaderService;
            _saveLoadService = saveLoadService;
            _staticData = staticData;
        }

        public async void Enter(string saveName)
        {
            _loaderService.CleanUp();

            _saveName = saveName;
            _loadingCurtain.SetLoading(0);

            await _loaderService.Initialize();
            _sceneLoader.Load(PLAY_CHARACTER_SCENE, OnSceneLoaded);
        }

        public void Exit()
        {
            _loadingCurtain.SetLoading(100);

        }

        private async void OnSceneLoaded()
        {
            _loadingCurtain.SetLoading(10);
            
            var levelData = LevelStaticData();
            var successLoad = await _saveLoadService.Load(_saveName, levelData, LoadProcessAction);

            if (!successLoad)
                _fsm.SpawnEvent((int)GameStateMachine.GameStateType.CreateGame);
            else
                _fsm.SpawnEvent((int) GameStateMachine.GameStateType.GameLoop);
        }

        private void LoadProcessAction(int loadPercent)
        {
            _loadingCurtain.SetLoading(20 + loadPercent / 1.25f); //20-100%
        }
    }
}