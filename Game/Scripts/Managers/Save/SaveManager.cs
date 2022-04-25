using System;
using System.Threading.Tasks;
using CharacterEditor.Services;
using Game;
using UnityEngine;

namespace CharacterEditor
{
    /*
     * Preservation of characters and characters in runtime
     */
    public class SaveManager : MonoBehaviour
    {
        public static string LOADED_SAVE_KEY = "loadedSaveKey" ;
        public GameObject saveLoadPopup;
        private SaveLoadPopup saveLoadPopupInstance;
        private GameSaveManager gameSaveManager;

        public static SaveManager Instance { get; private set; }
      

        void Awake()
        {
            if (Instance != null) Destroy(gameObject);
            Instance = this;
            gameSaveManager = new GameSaveManager(AllServices.Container.Single<ILoaderService>(), AllServices.Container.Single<ICoroutineRunner>());
        }

        public void TogglePopup(SaveLoadPopup.SaveLoadPopupMode mode)
        {
            if (saveLoadPopupInstance == null)
            {
                saveLoadPopupInstance = Instantiate(saveLoadPopup, GameObject.Find("Canvas").transform)
                    .GetComponent<SaveLoadPopup>();

                if (saveLoadPopupInstance == null) return;
                saveLoadPopupInstance.SetMode(mode);
                return;
            }
            saveLoadPopupInstance.SetMode(mode);
            saveLoadPopupInstance.Toggle();
        }

        public void OnSaveClick(string fileName = "")
        {
            Save(fileName);
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
            return gameSaveManager.GetSaves();
        }


        public void OnLoadClick(string fileName)
        {
            // gameSaveManager.Load(fileName, (l) => {});
        }


        /*
         * Save in runtime
         */
        private void Save(string saveName)
        {
            gameSaveManager.Save(saveName);
        }

        public async Task Load(string saveName, Action<int> loadProcessAction)
        {
            await gameSaveManager.Load(saveName, loadProcessAction);
        }
    }
}