using System.Linq;
using CharacterEditor.Services;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CharacterEditor
{
    public class SaveManager : MonoBehaviour
    {
        public GameObject saveLoadPopup;
        private SaveLoadPopup _saveLoadPopupInstance;

        private ISaveService _saveService;
        private IConfigManager _configManager;
        private IFSM _gameStateMachine;
        private Transform _canvas;

        public static SaveManager Instance { get; private set; }
      

        void Awake()
        {
            if (Instance != null) Destroy(gameObject);
            Instance = this;
            _saveService = AllServices.Container.Single<ISaveService>();
            _configManager = AllServices.Container.Single<IConfigManager>();
            _gameStateMachine = AllServices.Container.Single<IFSM>();

            _canvas = GameObject.Find("Canvas").transform;
        }

        public void TogglePopup(SaveLoadPopup.SaveLoadPopupMode mode)
        {
            if (_saveLoadPopupInstance == null)
            {
                CreateSaveLoadPopup(mode);
                return;
            }

            _saveLoadPopupInstance.SetMode(mode);
            _saveLoadPopupInstance.Toggle();
        }

        private void CreateSaveLoadPopup(SaveLoadPopup.SaveLoadPopupMode mode)
        {
            _saveLoadPopupInstance = Instantiate(saveLoadPopup, _canvas)
                .GetComponent<SaveLoadPopup>();

            _saveLoadPopupInstance.SetMode(mode);
        }

        public void OnSavePrefabClick()
        {
#if UNITY_EDITOR
            var staticDataService = AllServices.Container.Single<IStaticDataService>();
            var prefabSaveManager = new PrefabSaveManager(TextureManager.Instance, MeshManager.Instance, staticDataService, _configManager.Config, _configManager.ConfigData);
            prefabSaveManager.Save();
#endif
        }

        public string[] GetSaves()
        {
            return _saveService.GetSaves();
        }


        public void Load(string fileName)
        {
            _gameStateMachine.SpawnEvent((int) GameStateMachine.GameStateType.LoadGame, fileName);
        }


        public void Save(string saveName)
        {
            if (GameManager.Instance != null)
                _saveService.SaveGame(saveName, SceneManager.GetActiveScene().name, GameManager.Instance);
            else 
                _saveService.CreateGame(saveName, _configManager.Config.guid,
                TextureManager.Instance.CharacterTexture, TextureManager.Instance.CurrentCharacterPortrait,
                MeshManager.Instance.SelectedSkinMeshes.Select(mw => mw.Mesh), MeshManager.Instance.FaceTexture);
        }
    }
}