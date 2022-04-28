using System;
using System.Threading.Tasks;
using CharacterEditor.Services;
using Game;
using UnityEngine;

namespace CharacterEditor
{
    /*
     * Preservation of characters  in runtime
     */
    public class SaveManager : MonoBehaviour
    {
        public GameObject saveLoadPopup;
        private SaveLoadPopup _saveLoadPopupInstance;

        private ISaveLoadService _saveLoadService;
        private IConfigManager _configManager;
        private IFSM _gameStateMachine;

        public static SaveManager Instance { get; private set; }
      

        void Awake()
        {
            if (Instance != null) Destroy(gameObject);
            Instance = this;
            _saveLoadService = AllServices.Container.Single<ISaveLoadService>();
            _configManager = AllServices.Container.Single<IConfigManager>();
            _gameStateMachine = AllServices.Container.Single<IFSM>();
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
            _saveLoadPopupInstance = Instantiate(saveLoadPopup, GameObject.Find("Canvas").transform)
                .GetComponent<SaveLoadPopup>();

            _saveLoadPopupInstance.SetMode(mode);
        }

    

        public void OnSavePrefabClick()
        {
#if UNITY_EDITOR
            var prefabSaveManager = new PrefabSaveManager();
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

            _saveLoadService.Save(saveName, _configManager.ConfigData);
        }

        public async Task Load(string saveName, Action<int> loadProcessAction)
        {
            await _saveLoadService.Load(saveName, loadProcessAction);
        }
    }
}