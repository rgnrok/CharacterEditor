using System;
using System.Collections.Generic;
using CharacterEditor.CharacterInventory;
using CharacterEditor.Mesh;
using UnityEngine;

namespace CharacterEditor.Services
{
    class SaveService : SaveLoadService, ISaveService
    {
        private readonly ISaveLoadStorage _storage;

        public SaveService(ISaveLoadStorage saveLoadStorage)
        {
            _storage = saveLoadStorage;
        }

        public string[] GetSaves() =>
            _storage.GetSaves();

        public string GetLastSave()
        {
            var saveName = PlayerPrefs.GetString(LOADED_SAVE_KEY);
            if (string.IsNullOrEmpty(saveName)) return null;
            if (!_storage.IsSaveExist(saveName)) return null;

            return saveName;
        }

        public void CreateGame(string saveName, string characterGuid,
            Texture2D characterTexture, Sprite portrait,
            IEnumerable<CharacterMesh> faceMeshes, Texture2D faceMeshTexture)
        {
            var guid = Guid.NewGuid() + characterGuid;
            var portraitName = portrait.name.SanitizeName();
            var characterData = new CharacterSaveData(guid, characterGuid, portraitName);

            _storage.SaveCharacterTexture(saveName, characterData.guid, CHARACTER_SKIN_TEXTURE_NAME, characterTexture);

            if (faceMeshes != null)
            {
                foreach (var mesh in faceMeshes)
                    characterData.faceMeshItems[mesh.MeshType] = mesh.MeshPath;

                _storage.SaveCharacterTexture(saveName, characterData.guid, CHARACTER_FACE_TEXTURE_NAME, faceMeshTexture);
            }

            var saveData = new SaveData(saveName, characterData);
            _storage.SaveData(saveName, saveData);
            KeepLastSaveName(saveName);
        }

        public void SaveGame(string saveName, string levelName, GameManager gameManager)
        {
            if (gameManager == null) return;

            var charactersData = new CharacterSaveData[gameManager.Characters.Count];
            var i = 0;
            foreach (var character in gameManager.Characters.Values)
            {
                _storage.SaveCharacterTexture(saveName, character.Guid, CHARACTER_SKIN_TEXTURE_NAME, character.Texture);
                _storage.SaveCharacterTexture(saveName, character.Guid, CHARACTER_FACE_TEXTURE_NAME, character.FaceMeshTexture);

                var inventoryCells = gameManager.Inventory.GetInventoryCells(character.Guid);
                var inventoryItems = new List<string>(gameManager.Inventory.GetCharacterItems(character.Guid).Keys);
                charactersData[i] = new CharacterSaveData(character, inventoryCells, inventoryItems);
                i++;
            }

            var containers = new Dictionary<string, ContainerSaveData>();
            foreach (var openedContainer in gameManager.OpenedContainers)
            {
                var container = new ContainerSaveData(openedContainer.Value);
                containers[openedContainer.Key] = container;
            }

            var saveData = new SaveData
            {
                saveName = saveName,
                levelKey = levelName,
                characters = charactersData,
                selectedCharacterGuid = gameManager.CurrentCharacter.Guid,
                mainCharacterGuid = gameManager.MainCharacterGuid,
                containers = containers
            };

            _storage.SaveData(saveName, saveData);
            KeepLastSaveName(saveName);
        }
    }
}