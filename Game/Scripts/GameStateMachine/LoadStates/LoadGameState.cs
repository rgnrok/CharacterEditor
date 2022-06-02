using System.Threading.Tasks;
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
        private readonly ISaveLoadStorage _saveLoadStorage;
        private readonly IStaticDataService _staticData;
        private readonly IMergeTextureService _mergeTextureService;
        private readonly IRegisterService _registerService;
        private string _saveName;
        private ILoadSaveService _loadSaveService;
        private bool _isRegistered;

        private LevelStaticData LevelStaticData() =>
            _staticData.ForLevel(SceneManager.GetActiveScene().name);


        public LoadGameState(FSM fsm, SceneLoader sceneLoader, LoadingCurtain loadingCurtain,
            ILoaderService loaderService, ISaveLoadStorage saveLoadStorage, IStaticDataService staticData, IMergeTextureService mergeTextureService,
            IRegisterService registerService) 
        {
            _fsm = fsm;
            _sceneLoader = sceneLoader;
            _loadingCurtain = loadingCurtain;
            _loaderService = loaderService;
            _saveLoadStorage = saveLoadStorage;
            _staticData = staticData;
            _mergeTextureService = mergeTextureService;
            _registerService = registerService;
        }

        public async void Enter(string saveName)
        {
            _loaderService.CleanUp();

            await RegisterGameServices();

            _saveName = saveName;
            _loadingCurtain.SetLoading(0);

            await _loaderService.Initialize();
            _sceneLoader.Load(PLAY_CHARACTER_SCENE, OnSceneLoaded);
        }

        private async Task RegisterGameServices()
        {
            if (_isRegistered) return;

            var armorMaterial = await _loaderService.MaterialLoader.LoadByPath(AssetsConstants.ArmorMergeMaterialPathKey);
            var clothMaterial = await _loaderService.MaterialLoader.LoadByPath(AssetsConstants.ClothMergeMaterialPathKey);
            var defaultMaterial = await _loaderService.MaterialLoader.LoadByPath(AssetsConstants.DefaultMaterialPathKey);
            var equipItemService = new CharacterEquipItemService(_mergeTextureService, defaultMaterial, clothMaterial, armorMaterial);
            _registerService.Register<ICharacterEquipItemService>(equipItemService);

            var gameFactory = new GameFactory(_loaderService, equipItemService);
            _registerService.Register<IGameFactory>(gameFactory);

            _loadSaveService = new LoadSaveService(_saveLoadStorage, _loaderService, gameFactory);
            _registerService.Register<ILoadSaveService>(_loadSaveService);

            _registerService.Register<ICharacterMoveService>(new CharacterMoveService());
            _registerService.Register<ICharacterManageService>(new CharacterManageService());
         
            _isRegistered = true;
        }

        public void Exit()
        {
            _loadingCurtain.SetLoading(100);

        }

        private async void OnSceneLoaded()
        {
            _loadingCurtain.SetLoading(10);

            var levelData = LevelStaticData();
            var successLoad = await _loadSaveService.Load(_saveName, levelData, LoadProcessAction);

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