using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using CharacterEditor.AssetBundleLoader;
using CharacterEditor.CharacterInventory;
using CharacterEditor.Mesh;
using CharacterEditor.Services;
using EnemySystem;
using Game;
using UnityEngine;
using FileMode = System.IO.FileMode;
using Sprite = UnityEngine.Sprite;

namespace CharacterEditor
{
    public class SaveLoadService : ISaveLoadService
    {
        private const string CHARACTER_SKIN_TEXTURE_NAME = "Character_texture.png";
        private const string CHARACTER_FACE_TEXTURE_NAME = "Character_face_texture.png";
        private const string SAVE_FILE_NAME = "FileName.dat";

        private const string LOADED_SAVE_KEY = "loadedSaveKey";


        private SaveData _saveData;
        private readonly ILoaderService _loaderService;
        private readonly ICoroutineRunner _coroutineRunner;

        private readonly string _savePath = Application.persistentDataPath + "/Saves/";

        public SaveLoadService(ILoaderService loaderService, ICoroutineRunner coroutineRunner)
        {
            _loaderService = loaderService;
            _coroutineRunner = coroutineRunner;
        }

        public string[] GetSaves()
        {
            if (!Directory.Exists(_savePath))
            {
                Directory.CreateDirectory(_savePath);
                return new string[0];
            }

            var folders = Directory.GetDirectories(_savePath);
            var saveNames = new string[folders.Length];
            for (var i = 0; i < saveNames.Length; i++)
            {
                var folderParts = folders[i].Split('/');
                saveNames[i] = folderParts[folderParts.Length - 1];
            }
            return saveNames;
        }

        public void Save(string saveName, CharacterGameObjectData data)
        {
            if (GameManager.Instance != null)
                SaveGame(saveName);
            else
                CreateGame(
                    saveName, data, 
                    TextureManager.Instance.CharacterTexture, TextureManager.Instance.CharacterPortrait,
                    MeshManager.Instance.SelectedSkinMeshes, MeshManager.Instance.FaceTexture);

            PlayerPrefs.SetString(LOADED_SAVE_KEY, saveName);
        }

        public string GetLastSave()
        {
            var saveName = PlayerPrefs.GetString(LOADED_SAVE_KEY);
            var saveFilePath = $"{_savePath}{saveName}/{SAVE_FILE_NAME}";
            if (!File.Exists(saveFilePath))
                return null;

            return saveName;
        }

        public async Task<bool> Load(string saveName, Action<int> loadProcessAction)
        {
            PlayerPrefs.SetString(LOADED_SAVE_KEY, saveName);

            var saveFilePath = $"{_savePath}{saveName}/{SAVE_FILE_NAME}";
            if (!File.Exists(saveFilePath))
                return false;

            Logger.Log("Load file " + saveFilePath);

            var bf = new BinaryFormatter();
            var file = File.Open(saveFilePath, FileMode.Open);

            _saveData = (SaveData)bf.Deserialize(file);
            file.Close();

            loadProcessAction(10);
            return await LoadData(loadProcessAction);
        }

        private void CreateGame(string saveName, CharacterGameObjectData configData,
            Texture2D characterTexture, Sprite portrait,
            List<CharacterMesh> faceMeshes, Texture2D faceMeshTexture)
        {
            var saveFilePath = _savePath + saveName;
            Directory.CreateDirectory(saveFilePath);

            var guid = Guid.NewGuid() + configData.Config.guid;
            var portraitName = portrait.name.Replace("(Clone)", "").Trim();
            var characterData = new CharacterSaveData(guid, configData.Config.guid, portraitName);

            File.WriteAllBytes($"{saveFilePath}/{characterData.guid}_{CHARACTER_SKIN_TEXTURE_NAME}", characterTexture.EncodeToPNG());

            if (faceMeshes != null)
            {
                foreach (var mesh in faceMeshes)
                    characterData.faceMeshItems[mesh.MeshType] = mesh.MeshPath;

                File.WriteAllBytes($"{saveFilePath}/{characterData.guid}_{CHARACTER_FACE_TEXTURE_NAME}", faceMeshTexture.EncodeToPNG());
            }

            var bf = new BinaryFormatter();
            var file = File.Open($"{saveFilePath}/{SAVE_FILE_NAME}", FileMode.Create);
            var saveData = new SaveData(saveName, characterData);

            bf.Serialize(file, saveData);
            file.Close();
        }

        private void SaveGame(string saveName)
        {
            if (GameManager.Instance == null) return;

            var saveFilePath = _savePath + saveName;
            if (!Directory.Exists(saveFilePath))
                Directory.CreateDirectory(saveFilePath);

            var charactersData = new CharacterSaveData[GameManager.Instance.Characters.Count];
            var i = 0;
            foreach (var character in GameManager.Instance.Characters.Values)
            {
                File.WriteAllBytes($"{saveFilePath}/{character.guid}_{CHARACTER_SKIN_TEXTURE_NAME}", character.Texture.EncodeToPNG());
                File.WriteAllBytes($"{saveFilePath}/{character.guid}_{CHARACTER_FACE_TEXTURE_NAME}", character.FaceMeshTexture.EncodeToPNG());

                var inventoryCeils = GameManager.Instance.Inventory.GetInventoryCeils(character.guid);
                var inventoryItems = new List<string>(GameManager.Instance.Inventory.GetCharacterItems(character.guid).Keys);
                charactersData[i] = new CharacterSaveData(character, inventoryCeils, inventoryItems);
                i++;
            }

            var containers = new Dictionary<string, ContainerSaveData>();
            foreach (var openedContainer in GameManager.Instance.OpenedContainers)
            {
                var container = new ContainerSaveData(openedContainer.Value);
                containers[openedContainer.Key] = container;
            }

            var bf = new BinaryFormatter();
            var file = File.Open(saveFilePath + "/FileName.dat", FileMode.Create);

            var saveData = new SaveData();
            saveData.saveName = saveName;
            saveData.characters = charactersData;
            saveData.selectedCharacterGuid = GameManager.Instance.CurrentCharacter.guid;
            saveData.mainCharacterGuid = GameManager.Instance.MainCharacterGuid;
            saveData.containers = containers;

            bf.Serialize(file, saveData);
            file.Close();
        }


        private IEnumerator LoadNpc(Action callbackAction)
        {
            if (_saveData == null)
            {
                callbackAction.Invoke();
                yield break;
            }

            foreach (var point in GameManager.Instance.PlayerCharacterSpawnPoints)
            {
                var needLoad = true;
                foreach (var sCharacter in _saveData.characters)
                {
                    if (sCharacter.guid == point.CharacterConfigGuid)
                    {
                        needLoad = false;
                        break;
                    }
                }
                if (!needLoad) continue;

                var loadPlayer = false;
                _loaderService.PlayerCharacterLoader.LoadData(point.CharacterConfigGuid, (config) => LoadPlayerCharacterCallback(config, point.transform.position,
                    playerNpcCharacter =>
                    {
                        GameManager.Instance.SetNpcPlayerCharacter(playerNpcCharacter);
                        loadPlayer = true;
                    }));

                while (!loadPlayer) yield return null;
            }
            callbackAction.Invoke();
        }

        private async Task LoadCharacters(Action callbackAction)
        {
            if (_saveData == null)
            {
                callbackAction.Invoke();
                return;
            }

            var saveFilePath = _savePath + _saveData.saveName;
            foreach (var characterData in _saveData.characters)
            {
                var textureData = File.ReadAllBytes($"{saveFilePath}/{characterData.guid}_{CHARACTER_SKIN_TEXTURE_NAME}");
                var characterTexture = new Texture2D(Constants.SKIN_TEXTURE_ATLAS_SIZE, Constants.SKIN_TEXTURE_ATLAS_SIZE, TextureFormat.RGB24, false);
                characterTexture.LoadImage(textureData);

                var faceTextureData = File.ReadAllBytes($"{saveFilePath}/{characterData.guid}_{CHARACTER_FACE_TEXTURE_NAME}");
                var characterFaceMeshTexture = new Texture2D(Constants.SKIN_MESHES_ATLAS_SIZE, Constants.SKIN_MESHES_ATLAS_SIZE);
                characterFaceMeshTexture.LoadImage(faceTextureData);

                var characterLoad = false;
                Logger.Log("LoadConfig " + characterData.configGuid);
                var config = await _loaderService.ConfigLoader.LoadConfig(characterData.configGuid);
              
                        Debug.Log("LoadConfig callback");
                        _coroutineRunner.StartCoroutine(LoadConfigCallback(characterData, config, characterTexture, characterFaceMeshTexture, character =>
                        {
                            GameManager.Instance.AddCharacter(character);
                            characterLoad = true;
                        }));
                    
                
                while (!characterLoad) await Task.Yield();
            }

            //todo
            var fc = Camera.main.GetComponent<FollowCamera>();
            fc.SetFocus(GameManager.Instance.CurrentCharacter.GameObjectData.CharacterObject.transform, true, true);


            callbackAction.Invoke();
        }

        private IEnumerator LoadEnemies(Action callbackAction)
        {
            for (var i = 0; i < GameManager.Instance.EnemySpawnPoints.Length; i++)
            {
                var point = GameManager.Instance.EnemySpawnPoints[i];
                Debug.Log("Start Load Enemy ");

                var loadEnemy = false;
                _loaderService.EnemyLoader.LoadData(point.EnemyGuid, async (config) =>
                    {
                        var raceConfig = await _loaderService.ConfigLoader.LoadConfig(config.entityConfig.guid);
                            config.entityConfig = raceConfig;
                            config.skinnedMeshes = raceConfig.skinnedMeshes;
                            var guid = string.Format("{0}_{1}", config.guid, i);
                            _coroutineRunner.StartCoroutine(LoadEnemyCallbackCoroutine(guid, config,
                                point.transform.position,
                                enemy =>
                                {
                                    GameManager.Instance.AddEnemy(enemy);
                                    var moveComponent = enemy.EntityGameObject.GetComponent<PlayerMoveComponent>();
                                    if (moveComponent != null) moveComponent.Stop(true);
                                    loadEnemy = true;
                                }));
                        
                    });


                while (!loadEnemy) yield return null;
                Debug.Log("End Load Enemy ");
            }

            callbackAction.Invoke();
        }

        private IEnumerator LoadContainers(Action callbackAction)
        {
            if (_saveData == null)
            {
                callbackAction.Invoke();
                yield break;
            }

            foreach (var point in GameManager.Instance.ContainerSpawnPoints)
            {
                Debug.Log("Start Load Containers ");

                var loadContainer = false;
                _loaderService.ContainerLoader.LoadData(point.ContainerDataGuid, (config) => LoadContainerCallback(config, point.transform.position,
                    (container, containerConfig) =>
                    {
                        ContainerSaveData containerSaveData;
                        if (!_saveData.containers.TryGetValue(containerConfig.guid, out containerSaveData))
                        {
                            Dictionary<int, ItemData> items = new Dictionary<int, ItemData>();
                            containerSaveData = new ContainerSaveData();
                            containerSaveData.guid = containerConfig.guid;
                            for (var i = 0; i < containerConfig.initItems.Length; i++)
                                items[i] = containerConfig.initItems[i];

                            container.SetData(containerSaveData, items);
                            loadContainer = true;
                            return;
                        }

                        //load from save file items
                        var itemGuids = new List<string>();
                        foreach (var itemData in containerSaveData.items)
                            itemGuids.Add(itemData.Value.dataGuid);

                        _loaderService.ItemLoader.LoadData(itemGuids, (Dictionary<string, ItemData> items) =>
                        {
                            Dictionary<int, ItemData> containerItems = new Dictionary<int, ItemData>();
                            foreach (var itemDataPair in containerSaveData.items)
                            {
                                ItemData itemData;
                                if (!items.TryGetValue(itemDataPair.Value.dataGuid, out itemData)) continue;
                                containerItems[itemDataPair.Key] = itemData;
                            }
                            container.SetData(containerSaveData, containerItems);
                        });


                        loadContainer = true;
                    }));
                while (!loadContainer) yield return null;
                Debug.Log("End Load Containers ");
            }
            callbackAction.Invoke();
        }

        private async Task<bool> LoadData(Action<int> loadProcessAction)
        {
            if (_saveData == null) return false;
            var totalCount = 4;
            var current = 0;
            await LoadCharacters(() => UpdateLoadProcess(loadProcessAction, 1, 1));

            // _coroutineRunner.StartCoroutine(LoadNpc(() => UpdateLoadProcess(loadProcessAction, ++current, totalCount)));
            // await LoadCharacters(() => UpdateLoadProcess(loadProcessAction, ++current, totalCount));
            // _coroutineRunner.StartCoroutine(LoadEnemies(() => UpdateLoadProcess(loadProcessAction, ++current, totalCount)));
            // _coroutineRunner.StartCoroutine(LoadContainers(() => UpdateLoadProcess(loadProcessAction, ++current, totalCount)));

            return true;
        }

        private void UpdateLoadProcess(Action<int> loadProcessAction, int current, int total)
        {
            var percentage = current * 100 / total;
            if (percentage == 100)
                _coroutineRunner.StartCoroutine(FinalLoad());

            loadProcessAction(current * 100 / total);
        }

        private IEnumerator FinalLoad()
        {
            while (!ItemManager.Instance.IsReady)
                yield return null;
            GameManager.Instance.MainCharacterGuid = _saveData.mainCharacterGuid;
            GameManager.Instance.SetCharacter(_saveData.selectedCharacterGuid);
        }


        private IEnumerator LoadEnemyCallbackCoroutine(string guid, EnemyConfig config, Vector3 position,
            Action<Enemy> callback)
        {
            var loadTexture = false;
            var loadFaceTexture = false;
            var loadArmorTexture = false;
            var loadPortraitIcon = false;

            Texture2D skinTexture = null;
            Texture2D faceTexture = null;
            Texture2D armorTexture = null;
            Sprite portraitIcon = null;
            Debug.Log("Start Load Textures");
            _loaderService.TextureLoader.LoadByPath(config.textureBundlePath, ( path, texture) =>
            {
                loadTexture = true;
                skinTexture = texture;
            });
            _loaderService.TextureLoader.LoadByPath(config.faceMeshTextureBundlePath, ( path, texture) =>
            {
                loadFaceTexture = true;
                faceTexture = texture;
            });
            _loaderService.TextureLoader.LoadByPath(config.armorTextureBundlePath, ( path, texture) =>
            {
                loadArmorTexture = true;
                armorTexture = texture;
            });
            _loaderService.IconLoader.LoadPortrait(config.portraitIconName, (portrait) =>
            {
                loadPortraitIcon = true;
                portraitIcon = portrait;
            });
            while (!loadTexture || !loadFaceTexture || !loadArmorTexture || !loadPortraitIcon)
            {
                yield return null;
            }
            Debug.Log("End Load textures");
            var materialLoader = new CommonLoader<Material>(_coroutineRunner);
            var loadMaterial = false;
            Material armorMaterial = null;
            Material faceMaterial = null;
            Material skinMaterial = null;
            materialLoader.LoadByPath(config.materialBundlePath, (path, material) =>
            {
                loadMaterial = true;
                armorMaterial = new Material(material);
                faceMaterial = new Material(material);
                skinMaterial = new Material(material);

            });
            while (!loadMaterial)
            {
                yield return null;
            }


            var go = SaveManager.Instantiate(config.entityConfig.EnemyPrefab, position, Quaternion.identity);
            go.layer = Constants.LAYER_ENEMY;
            var goData = new EnemyGameObjectData(config, go);

            if (armorMaterial == null)
            {
                armorMaterial = new Material(goData.SkinMeshes[0].material);
                faceMaterial = new Material(goData.SkinMeshes[0].material);
                skinMaterial = new Material(goData.SkinMeshes[0].material);
            }
            armorMaterial.mainTexture = armorTexture;
            faceMaterial.mainTexture = faceTexture;
            skinMaterial.mainTexture = skinTexture;



            foreach (var skinMesh in goData.SkinMeshes)
            {
                skinMesh.material = skinMaterial;
            }

            //            goData.f
            var enemy = Enemy.Create(guid, goData, null, portraitIcon);

            var prefabs = new List<string>();
            foreach (var bone in config.prefabBoneData.armorBones)
                prefabs.Add(bone.prefabBundlePath);
            foreach (var bone in config.prefabBoneData.faceBones)
                prefabs.Add(bone.prefabBundlePath);



            var objectLoader = new CommonLoader<GameObject>(_coroutineRunner);
            var loadItems = false;
            objectLoader.LoadByPath(prefabs, (objects) =>
            {

                foreach (var bone in config.prefabBoneData.armorBones)
                {
                    GameObject prefabGo;
                    if (!objects.TryGetValue(bone.prefabBundlePath, out prefabGo)) continue;

                    var rootBone = Helper.FindTransform(go.transform, bone.bone);
                    if (rootBone != null)
                    {
                        var meshObject = GameObject.Instantiate(prefabGo, rootBone.transform.position, rootBone.transform.rotation, rootBone);
                        foreach (var meshRenderer in meshObject.GetComponentsInChildren<MeshRenderer>())
                        {
                            meshRenderer.material = armorMaterial;
                        }
                    }
                }

                foreach (var bone in config.prefabBoneData.faceBones)
                {
                    GameObject prefabGo;
                    if (!objects.TryGetValue(bone.prefabBundlePath, out prefabGo)) continue;

                    var rootBone = Helper.FindTransform(go.transform, bone.bone);
                    if (rootBone != null)
                    {
                        var meshObject = GameObject.Instantiate(prefabGo, rootBone.transform.position, rootBone.transform.rotation, rootBone);
                        foreach (var meshRenderer in meshObject.GetComponentsInChildren<MeshRenderer>())
                        {
                            meshRenderer.material = faceMaterial;
                        }
                    }
                }

                loadItems = true;
            });
            while (!loadItems) yield return null;
            callback.Invoke(enemy);
        }

        private void LoadContainerCallback(ContainerConfig config, Vector3 position,
            Action<Container, ContainerConfig> callback)
        {
            _coroutineRunner.StartCoroutine(LoadContainerCallbackCoroutine(config, position, callback));
        }

        private IEnumerator LoadContainerCallbackCoroutine(ContainerConfig config, Vector3 position,
         Action<Container, ContainerConfig> callback)
        {
            var objectLoader = new CommonLoader<GameObject>(_coroutineRunner);
            var loadGo = false;
            Container container = null;
            objectLoader.LoadByPath(config.bundlePrefabPath, (path, obj) =>
            {
                var containerGo = GameObject.Instantiate(obj, position, Quaternion.identity);
                if (containerGo != null) container = containerGo.GetComponent<Container>();
                loadGo = true;
            });
            while (!loadGo) yield return null;
            callback.Invoke(container, config);
        }


        private async Task LoadPlayerCharacterCallback(PlayerCharacterConfig config, Vector3 position, Action<Character> callback)
        {
            //Need reload character config for correct load main and preview prefabs
            var raceConfig = await _loaderService.ConfigLoader.LoadConfig(config.characterConfig.guid);
                config.characterConfig = raceConfig;
                _coroutineRunner.StartCoroutine(LoadPlayerCharacterCallbackCoroutine(config, position, callback));

        }


        private IEnumerator LoadPlayerCharacterCallbackCoroutine(PlayerCharacterConfig config, Vector3 position, Action<Character> callback)
        {
            var loadTexture = false;
            var loadFaceTexture = false;
            var loadPortraitIcon = false;

            Texture2D skinTexture = null;
            Texture2D faceTexture = null;
            Sprite portraitIcon = null;
            _loaderService.TextureLoader.LoadByPath(config.textureBundlePath, (path, texture) =>
            {
                loadTexture = true;
                skinTexture = texture;
            });
            _loaderService.TextureLoader.LoadByPath(config.faceMeshTextureBundlePath, (path, texture) =>
            {
                loadFaceTexture = true;
                faceTexture = texture;
            });
            _loaderService.IconLoader.LoadPortrait(config.portraitIconName, (portrait) =>
            {
                loadPortraitIcon = true;
                portraitIcon = portrait;
            });

            while (!loadTexture || !loadFaceTexture || !loadPortraitIcon)
            {
                yield return null;
            }


            var go = GameObject.Instantiate(config.characterConfig.Prefab, position, Quaternion.identity);
            go.layer = Constants.LAYER_NPC;
            var goData = new CharacterGameObjectData(config.characterConfig, go, null);
            var character = new Character(config.guid, goData, skinTexture, faceTexture, portraitIcon);


            // Load item textures and meshes
            var equipItems = new Dictionary<EquipItemSlot, EquipItem>();
            var faceMeshItems = new Dictionary<MeshType, FaceMesh>();

            //            //todo only equipItems
            //            foreach (var itemData in characterData.inventoryCeils.Values)
            //            {
            //                ItemData data;
            //                if (!items.TryGetValue(itemData.meshGuid, out data)) continue;
            //
            //                var eiMesh = new EquipItemMesh((EquipItemData)data, LoaderManager.Instance.TextureLoader, LoaderManager.Instance.MeshLoader);
            //                GameManager.Instance.Inventory.InventoryItems[itemData.guid] = new EquipItem(itemData.guid, data, eiMesh, itemData.stats);
            //            }
            //
            //            // Equip items from inventory
            foreach (var itemInfo in config.equipItems)
            {

                //todo use ready created item?
                var eiMesh = new EquipItemMesh((EquipItemData)itemInfo.item, _loaderService.TextureLoader, _loaderService.MeshLoader);
                equipItems[itemInfo.itemSlot] = new EquipItem(itemInfo.item, eiMesh);
            }

            foreach (var faceMesh in config.faceMeshs)
                faceMeshItems[faceMesh.meshType] = new FaceMesh(_loaderService.MeshLoader, faceMesh.meshType, faceMesh.meshBundlePath);

            var eiCoroutine = _coroutineRunner.StartCoroutine(EquipItems(character, equipItems, faceMeshItems));

            yield return eiCoroutine;
            callback.Invoke(character);

        }


        private IEnumerator LoadConfigCallback(CharacterSaveData characterData, CharacterConfig config, Texture2D cTexture, Texture2D fTexture, Action<Character> loadCharacterCallback)
        {
            Debug.Log("LoadConfigCallback");
            if (config == null) yield break;

            var loadPortraitIcon = false;
            Sprite portraitIcon = null;
            _loaderService.IconLoader.LoadPortrait(characterData.portrait, (portrait) =>
            {
                loadPortraitIcon = true;
                portraitIcon = portrait;
            });

            while (!loadPortraitIcon) yield return null;

            //Setup config
            var previewPrefab = GameObject.Instantiate(config.PreviewPrefab);
            previewPrefab.transform.position = Vector3.zero;
            previewPrefab.SetActive(false);

            var characterObject = GameObject.Instantiate(config.Prefab, characterData.position, characterData.rotation);
            characterObject.SetActive(false);


            var gameobjectData = new CharacterGameObjectData(config, characterObject, previewPrefab);

            //Prepare items for load
            var character = new Character(characterData, gameobjectData, cTexture, fTexture, portraitIcon);
            var itemGuids = new List<string>();
            // Надетые айтемы
            foreach (var pair in characterData.equipItems)
                itemGuids.Add(pair.Value.dataGuid);
            // Айтемы в инвентаре
            foreach (var item in characterData.inventoryCells.Values)
                itemGuids.Add(item.dataGuid);


            Coroutine eiCoroutine = null;
            _loaderService.ItemLoader.LoadData(itemGuids, (Dictionary<string, ItemData> items) =>
            {
                // Load item textures and meshes
                var equipItems = new Dictionary<EquipItemSlot, EquipItem>();
                var faceMeshItems = new Dictionary<MeshType, FaceMesh>();

                //todo only equipItems
                foreach (var ceilPair in characterData.inventoryCells)
                {
                    var itemData = ceilPair.Value;

                    ItemData data;
                    if (!items.TryGetValue(itemData.dataGuid, out data)) continue;

                    var eiMesh = new EquipItemMesh((EquipItemData)data, _loaderService.TextureLoader, _loaderService.MeshLoader);
                    GameManager.Instance.Inventory.SetItemToInvetory(characterData.guid, new EquipItem(itemData.guid, data, eiMesh, itemData.stats), ceilPair.Key);
                }

                // Equip items from inventory
                foreach (var pair in characterData.equipItems)
                {
                    ItemData data;
                    if (!items.TryGetValue(pair.Value.dataGuid, out data)) continue;
                    //todo use ready created item?
                    var eiMesh = new EquipItemMesh((EquipItemData)data, _loaderService.TextureLoader, _loaderService.MeshLoader);
                    equipItems[pair.Key] = new EquipItem(pair.Value.guid, data, eiMesh, pair.Value.stats);
                }

                foreach (var pair in characterData.faceMeshItems)
                    faceMeshItems[pair.Key] = new FaceMesh(_loaderService.MeshLoader, pair.Key, pair.Value);

                eiCoroutine = SaveManager.Instance.StartCoroutine(EquipItems(character, equipItems, faceMeshItems));
            });

            while (eiCoroutine == null) yield return null;
            yield return eiCoroutine;

            loadCharacterCallback.Invoke(character);
        }

        private IEnumerator EquipItems(Character character, Dictionary<EquipItemSlot, EquipItem> equipItems, Dictionary<MeshType, FaceMesh> faceMeshItems)
        {

            //            Inventory.Init(character); // todo need?

            foreach (var feaceMeshPair in faceMeshItems)
            {
                feaceMeshPair.Value.LoadTextureAndMesh(character.configGuid);
                while (!feaceMeshPair.Value.ItemMesh.IsReady) yield return null;
                character.AddFaceMesh(feaceMeshPair.Value);
            }

            while (!ItemManager.Instance.IsReady) yield return null;
            ItemManager.Instance.SetCharacter(character);
            ItemManager.Instance.EquipItems(equipItems);
            while (!ItemManager.Instance.IsReady) yield return null;
        }
    }
}