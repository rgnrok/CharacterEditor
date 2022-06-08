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
        private readonly IStaticDataService _staticData;
        private readonly ICharacterEquipItemService _equipItemService;
        private ISaveLoadService _saveLoadService;
        private string _saveName;
        private bool _isRegistered;

        private LevelStaticData LevelStaticData() =>
            _staticData.ForLevel(SceneManager.GetActiveScene().name);


        public LoadGameState(FSM fsm, SceneLoader sceneLoader, LoadingCurtain loadingCurtain,
            ILoaderService loaderService, ISaveLoadService saveLoadService, IStaticDataService staticData, IMergeTextureService mergeTextureService,
            ICharacterEquipItemService equipItemService) 
        {
            _fsm = fsm;
            _sceneLoader = sceneLoader;
            _loadingCurtain = loadingCurtain;
            _loaderService = loaderService;
            _saveLoadService = saveLoadService;
            _staticData = staticData;
            _equipItemService = equipItemService;
        }

        public async void Enter(string saveName)
        {
            _loaderService.CleanUp();
            
            _saveName = saveName;
            _loadingCurtain.SetLoading(0);

            await _loaderService.Initialize();

            await LoadEquipServiceMaterials();

            _sceneLoader.Load(PLAY_CHARACTER_SCENE, OnSceneLoaded);
        }

        private async Task LoadEquipServiceMaterials()
        {
            var materials = _loaderService.DataManager.ParseGameMaterials();
            var armorRenderShaderMaterial = await _loaderService.MaterialLoader.LoadByPath(materials[AssetsConstants.ArmorMergeMaterialPathKey]);
            var clothRenderShaderMaterial = await _loaderService.MaterialLoader.LoadByPath(materials[AssetsConstants.ClothMergeMaterialPathKey]);
            var modelMaterial = await _loaderService.MaterialLoader.LoadByPath(materials[AssetsConstants.ModelMaterialPathKey]);
            var previewMaterial = await _loaderService.MaterialLoader.LoadByPath(materials[AssetsConstants.PreviewMaterialPathKey]);

            _equipItemService.SetupMaterials(armorRenderShaderMaterial, clothRenderShaderMaterial, modelMaterial, previewMaterial);
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