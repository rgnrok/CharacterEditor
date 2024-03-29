﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CharacterEditor.CharacterInventory;
using CharacterEditor.Mesh;
using CharacterEditor.Services;
using CharacterEditor.StaticData;
using EnemySystem;
using UnityEngine;
using Sprite = UnityEngine.Sprite;

namespace CharacterEditor
{
    public class SaveLoadService : ISaveLoadService
    {
        private const string CHARACTER_SKIN_TEXTURE_NAME = "Character_texture.png";
        private const string CHARACTER_FACE_TEXTURE_NAME = "Character_face_texture.png";
        private const string LOADED_SAVE_KEY = "loadedSaveKey";

        private readonly ISaveLoadStorage _storage;
        private readonly ILoaderService _loaderService;
        private readonly IGameFactory _gameFactory;

        private List<Task<bool>> _loadTasks;

        public event Action<SaveData> OnLoadData;
        public event Action<IList<Character>> OnCharactersLoaded;
        public event Action<IList<Character>> OnPlayableNpcLoaded;
        public event Action<IList<Enemy>> OnEnemiesLoaded;

        public SaveLoadService(ISaveLoadStorage saveLoadStorage, ILoaderService loaderService, IGameFactory gameFactory)
        {
            _storage = saveLoadStorage;
            _loaderService = loaderService;
            _gameFactory = gameFactory;
        }

        private void KeepLastSaveName(string saveName) => 
            PlayerPrefs.SetString(LOADED_SAVE_KEY, saveName);

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

        public async Task<bool> Load(string saveName, LevelStaticData levelData, Action<int> loadProcessAction)
        {
            KeepLastSaveName(saveName);

            var saveData = _storage.LoadData(saveName);
            if (saveData == null) return false;

            var result = await LoadData(saveData, levelData, loadProcessAction);
            if (result) OnLoadData?.Invoke(saveData);

            return result;
        }

        private async Task<bool> LoadData(SaveData saveData, LevelStaticData levelData, Action<int> loadProcessAction)
        {
            _loadTasks = new List<Task<bool>>
            {
                LoadPlayableNpc(saveData, levelData),
                LoadEnemies(saveData, levelData),
                LoadContainers(saveData, levelData),
                LoadCharacters(saveData, levelData),

            };

            var completedTasks = 0;
            var totalCount = _loadTasks.Count;

            var result = true;
            while (_loadTasks.Count != 0)
            {
                await Task.Yield();
                foreach (var taskData in _loadTasks)
                {
                    if (!taskData.IsCompleted) continue;

                    result &= taskData.Result;
                    if (!result) return false;

                    _loadTasks.Remove(taskData);
                    completedTasks++;

                    UpdateLoadProcess(loadProcessAction, completedTasks, totalCount);
                    break;
                }
            }
            return true;
        }

        private async Task<bool> LoadCharacters(SaveData saveData, LevelStaticData levelData)
        {
            if (saveData == null) return false;

            var characters = new List<Character>(saveData.characters.Length);
            foreach (var characterData in saveData.characters)
            {
                var position = GetCharacterPosition(characterData, saveData, levelData);

                var characterTexture = _storage.LoadCharacterTexture(saveData.saveName, characterData.guid, CHARACTER_SKIN_TEXTURE_NAME, Constants.SKIN_TEXTURE_ATLAS_SIZE);
                var characterFaceMeshTexture = _storage.LoadCharacterTexture(saveData.saveName, characterData.guid, CHARACTER_FACE_TEXTURE_NAME, Constants.SKIN_MESHES_ATLAS_SIZE);

                var config = await _loaderService.ConfigLoader.LoadConfig(characterData.configGuid);
                var character = await _gameFactory.CreateGameCharacter(characterData, config, characterTexture, characterFaceMeshTexture, position);

                characters.Add(character);
            }
            OnCharactersLoaded?.Invoke(characters);
            return true;
        }

        private Vector3 GetCharacterPosition(CharacterSaveData characterData, SaveData saveData, LevelStaticData levelData)
        {
            if (characterData.guid != saveData.mainCharacterGuid) return characterData.position;

            if (string.IsNullOrEmpty(saveData.levelKey) || saveData.levelKey != levelData.LevelKey) return levelData.InitialPlayerPoint;
            return characterData.position;
        }

        private async Task<bool> LoadPlayableNpc(SaveData saveData, LevelStaticData levelData)
        {
            if (saveData == null) return false;

            var npcs = new List<Character>();
            foreach (var point in levelData.PlayableNpcSpawners)
            {
                var needLoad = saveData.characters.All(sCharacter => sCharacter.guid != point.ConfigId);
                if (!needLoad) continue;

                var config = await _loaderService.PlayableNpcLoader.LoadData(point.ConfigId);
                var skinTexture = await _loaderService.TextureLoader.LoadByPath(_loaderService.PathDataProvider.GetPath(config.texturePath));
                var faceTexture = await _loaderService.TextureLoader.LoadByPath(_loaderService.PathDataProvider.GetPath(config.faceMeshTexturePath));
                var portraitIcon = await _loaderService.SpriteLoader.LoadPortrait(config.portraitIconName);

                var playerNpcCharacter = await _gameFactory.CreatePlayableNpc(config, skinTexture, faceTexture, portraitIcon, point.Position);
                npcs.Add(playerNpcCharacter);
            }
            OnPlayableNpcLoaded?.Invoke(npcs);

            return true;
        }

        private async Task<bool> LoadEnemies(SaveData saveData, LevelStaticData levelData)
        {
            var enemies = new List<Enemy>(levelData.EnemySpawners.Count);
            foreach (var enemySpawner in levelData.EnemySpawners)
            {
                var enemyConfig = await _loaderService.EnemyLoader.LoadData(enemySpawner.ConfigId);
                var enemy = await LoadEnemy(enemySpawner.Id, enemyConfig, enemySpawner.Position);
                enemies.Add(enemy);
            }
            OnEnemiesLoaded?.Invoke(enemies);

            return true;
        }

        private async Task<bool> LoadContainers(SaveData saveData, LevelStaticData levelData)
        {
            if (saveData == null) return false;

            foreach (var containerSpawner in levelData.ContainerSpawners)
            {
                var containerConfig = await _loaderService.ContainerLoader.LoadData(containerSpawner.ConfigId);
                saveData.containers.TryGetValue(containerConfig.guid, out var containerSaveData);

                await _gameFactory.CreateContainer(containerConfig, containerSaveData, containerSpawner.Position);
            }

            return true;
        }

        private async Task<Enemy> LoadEnemy(string guid, EnemyConfig config, Vector3 position)
        {
            var skinTextureTask = _loaderService.TextureLoader.LoadByPath(_loaderService.PathDataProvider.GetPath(config.texturePath));
            var faceTextureTask = _loaderService.TextureLoader.LoadByPath(_loaderService.PathDataProvider.GetPath(config.faceMeshTexturePath));
            var armorTextureTask = _loaderService.TextureLoader.LoadByPath(_loaderService.PathDataProvider.GetPath(config.armorTexturePath));
            var portraitIconTask = _loaderService.SpriteLoader.LoadPortrait(config.portraitIconName);

            await Task.WhenAll(skinTextureTask, faceTextureTask, armorTextureTask, portraitIconTask);
            var skinTexture = skinTextureTask.Result;
            var faceTexture = faceTextureTask.Result;
            var armorTexture = armorTextureTask.Result;
            var portraitIcon = portraitIconTask.Result;

            var material = await _loaderService.MaterialLoader.LoadByPath(_loaderService.PathDataProvider.GetPath(config.materialPath));

            return await _gameFactory.CreateEnemy(guid, config, material, skinTexture, faceTexture, armorTexture, portraitIcon, position);
        }

        private void UpdateLoadProcess(Action<int> loadProcessAction, int current, int total)
        {
            loadProcessAction(current * 100 / total);
        }

    }
}