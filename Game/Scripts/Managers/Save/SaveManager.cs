using System.Linq;
using CharacterEditor.Services;
using UnityEngine;

namespace CharacterEditor
{
    public class SaveManager : MonoBehaviour
    {
        public GameObject saveLoadPopup;
        private SaveLoadPopup _saveLoadPopupInstance;

        private ISaveLoadService _saveLoadService;
        private IConfigManager _configManager;
        private IFSM _gameStateMachine;
        private Transform _canvas;

        public static SaveManager Instance { get; private set; }
      

        void Awake()
        {
            if (Instance != null) Destroy(gameObject);
            Instance = this;
            _saveLoadService = AllServices.Container.Single<ISaveLoadService>();
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
            return _saveLoadService.GetSaves();
        }


        public void Load(string fileName)
        {
            _gameStateMachine.SpawnEvent((int) GameStateMachine.GameStateType.LoadGame, fileName);
        }


        public void Save(string saveName)
        {
            if (GameManager.Instance != null)
                _saveLoadService.SaveGame(saveName, GameManager.Instance);
            else 
                _saveLoadService.CreateGame(saveName, _configManager.ConfigData,
                TextureManager.Instance.CharacterTexture, TextureManager.Instance.CharacterPortrait,
                MeshManager.Instance.SelectedSkinMeshes.Select(mw => mw.Mesh), MeshManager.Instance.FaceTexture);
        }
    }
}