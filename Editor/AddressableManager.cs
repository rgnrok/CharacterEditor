using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CharacterEditor;
using CharacterEditor.AssetDatabaseLoader;
using CharacterEditor.CharacterInventory;
using CharacterEditor.JSONMap;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Editor
{
    public class AddressableManager
    {
        private const string PREFABS_GROUP_NAME = "Prefabs";
        private const string CONFIG_GROUP_NAME = "Configs";
        private const string TEXTURE_GROUP_NAME = "Textures";
        private const string MESH_GROUP_NAME = "Meshes";

        private const string ITEMS_GROUP_NAME = "Items";
        private const string ITEM_PREFABS_GROUP_NAME = "Item_Prefabs";
        private const string NPC_GROUP_NAME = "PlayableNpc";
        private const string ENEMIES_GROUP_NAME = "Enemies";
        private const string CONTAINERS_GROUP_NAME = "Containers";

        private const string ICONS_GROUP_NAME = "Icons";
        private const string CURSORS_GROUP_NAME = "Cursors";

        [MenuItem("Tools/Character Editor/Refresh Addressables")]
        public static void RefreshAddressable()
        {
            var loader = new ConfigLoader();
            var configs = loader.LoadConfigs().Result;
            try
            {
                UpdateAddressables(configs);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error: {e.Message}\n {e.StackTrace}");
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }


        private static void UpdateAddressables(CharacterConfig[] configs)
        {
            ClearAddressables();

            var dataMap = new DataMap();

            dataMap.races = ParseRacesConfigs(configs);

            dataMap.items = ParseItems();
            dataMap.playableNpc = ParsePlayableNpc();
            dataMap.enemies = ParseEnemies();
            dataMap.containers = ParseContainers();
            UpdateIcons();

            DisplayProgressBar("Save addressablesInfo", "", 0.9f);

            if (!Directory.Exists(Application.dataPath + "/Resources/"))
                Directory.CreateDirectory(Application.dataPath + "/Resources/");

            using (var fs = new FileStream("Assets/Resources/addressablesInfo.json", FileMode.Create))
            {
                using (var writer = new StreamWriter(fs))
                {
                    writer.Write(JsonUtility.ToJson(dataMap));
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.ClearProgressBar();
        }

        private static void UpdateIcons()
        {
            SetupAddressable(SpriteLoader.ItemIconAtlasPath, ICONS_GROUP_NAME, CharacterEditor.AddressableLoader.SpriteLoader.ITEM_ICON_ATLAS_BUNDLE_NAME);
            SetupAddressable(SpriteLoader.PortraitIconAtlasPath, ICONS_GROUP_NAME, CharacterEditor.AddressableLoader.SpriteLoader.PORTRAIT_ICON_ATLAS_BUNDLE_NAME);

            foreach (CursorType cursorType in Enum.GetValues(typeof(CursorType)))
            {
                var cursorName = Helper.GetCursorTextureNameByType(cursorType);
                if (string.IsNullOrEmpty(cursorName)) continue;

                SetupAddressable(CursorLoader.GetCursorPath(cursorName), CURSORS_GROUP_NAME, CharacterEditor.AddressableLoader.CursorLoader.GetCursorPath(cursorName));
            }
        }

        private static List<GuidPathMap> ParseContainers()
        {
            var containerLoader = new ContainerLoader();
            var map = ParseDataEntitiesPath(containerLoader, CONTAINERS_GROUP_NAME);
            foreach (var containerData in containerLoader.LoadData().Values)
            {
                containerData.prefab.addressPath = SetupAddressable(containerData.prefab.path, CONTAINERS_GROUP_NAME);
                EditorUtility.CopySerialized(containerData, containerData);
            }
            AssetDatabase.SaveAssets();

            return map;
        }

        private static List<GuidPathMap> ParseEnemies()
        {
            var enemyLoader = new EnemyLoader();
            var map = ParseDataEntitiesPath(enemyLoader, ENEMIES_GROUP_NAME);
            foreach (var enemyData in enemyLoader.LoadData().Values)
            {
                var enemyGroup = $"{ENEMIES_GROUP_NAME}_{enemyData.guid}";
                enemyData.texturePath.addressPath = SetupAddressable(enemyData.texturePath.path, enemyGroup);
                enemyData.faceMeshTexturePath.addressPath = SetupAddressable(enemyData.faceMeshTexturePath.path, enemyGroup);
                enemyData.armorTexturePath.addressPath = SetupAddressable(enemyData.armorTexturePath.path, enemyGroup);
                enemyData.materialPath.addressPath = SetupAddressable(enemyData.materialPath.path, enemyGroup);

                SetupAddressable(enemyData.prefabBoneData, enemyGroup);
                foreach (var faceBone in enemyData.prefabBoneData.faceBones)
                    faceBone.prefabPath.addressPath = GetAddressablePath(faceBone.prefabPath.path);
                foreach (var armorBone in enemyData.prefabBoneData.armorBones)
                    armorBone.prefabPath.addressPath = GetAddressablePath(armorBone.prefabPath.path);
                EditorUtility.CopySerialized(enemyData.prefabBoneData, enemyData.prefabBoneData);


                var prefabPath = GetAddressablePath(enemyData.prefabPath.path);
                if (string.IsNullOrEmpty(prefabPath))
                    prefabPath = SetupAddressable(enemyData.prefabPath.path, enemyGroup);
                enemyData.prefabPath.addressPath = prefabPath;

                EditorUtility.CopySerialized(enemyData, enemyData);
            }
            AssetDatabase.SaveAssets();

            return map;
        }

        private static List<GuidPathMap> ParsePlayableNpc()
        {
            var playableNpcLoader = new PlayableNpcLoader();
            var map = ParseDataEntitiesPath(playableNpcLoader, NPC_GROUP_NAME);
            foreach (var npcData in playableNpcLoader.LoadData().Values)
            {
                npcData.texturePath.addressPath = SetupAddressable(npcData.texturePath.path, NPC_GROUP_NAME);
                npcData.faceMeshTexturePath.addressPath = SetupAddressable(npcData.faceMeshTexturePath.path, NPC_GROUP_NAME);

                foreach (var faceMesh in npcData.faceMeshs)
                    faceMesh.meshPath.addressPath = GetAddressablePath(faceMesh.meshPath.path);

                EditorUtility.CopySerialized(npcData, npcData);
            }
            AssetDatabase.SaveAssets();

            return map;
        }

        private static List<GuidPathMap> ParseItems()
        {
            var itemLoader = new ItemLoader();
            var map = ParseDataEntitiesPath(itemLoader, ITEMS_GROUP_NAME);
            
            foreach (var itemData in itemLoader.LoadData().Values)
            {
                if (!string.IsNullOrEmpty(itemData.prefab.path))
                {
                    itemData.prefab.addressPath = SetupAddressable(itemData.prefab.path, ITEM_PREFABS_GROUP_NAME);
                    EditorUtility.CopySerialized(itemData, itemData);
                }

                if (itemData is EquipItemData equipItem)
                    UpdateEquipItemPaths(equipItem);
            }
            AssetDatabase.SaveAssets();

            return map;
        }

        private static void UpdateEquipItemPaths(EquipItemData equipItemData)
        {
            foreach (var equipItem in equipItemData.configsItems)
            {
                foreach (var textureInfo in equipItem.textures)
                    textureInfo.texture.addressPath = GetAddressablePath(textureInfo.texture.path);

                foreach (var prefabAndTextureInfo in equipItem.models)
                {
                    // Main Texture (right hand)
                    prefabAndTextureInfo.texture.addressPath = GetAddressablePath(prefabAndTextureInfo.texture.path);
                    // Additional Texture (left hand)
                    prefabAndTextureInfo.additionalTexture.addressPath = GetAddressablePath(prefabAndTextureInfo.additionalTexture.path);

                    //Convert Main Prefab path (right)
                    prefabAndTextureInfo.prefab.addressPath = GetAddressablePath(prefabAndTextureInfo.prefab.path);
                    //Convert Additional Prefab path (left)
                    prefabAndTextureInfo.additionalPrefab.addressPath = GetAddressablePath(prefabAndTextureInfo.additionalPrefab.path);
                }
            }
            EditorUtility.CopySerialized(equipItemData, equipItemData);
        }

        private static List<RaceMap> ParseRacesConfigs(CharacterConfig[] configs)
        {
            var configsCount = configs.Length;
            var races = new List<RaceMap>(configsCount);
            for (var i = 0; i < configsCount; i++)
            {
                var raceName = configs[i].folderName;
                DisplayProgressBar("ParseRacesConfigs", raceName, (float) i / configsCount);

                var raceMap = new RaceMap
                {
                    race = raceName,
                    configPath = SetupAddressable(configs[i], CONFIG_GROUP_NAME),
                    prefabPath = SetupAddressable(configs[i].prefabPath.path, PREFABS_GROUP_NAME),
                    createGamePrefabPath = SetupAddressable(configs[i].createGamePrefabPath.path, PREFABS_GROUP_NAME),
                    previewPrefabPath = SetupAddressable(configs[i].previewPrefabPath.path, PREFABS_GROUP_NAME),
                    configGuid = configs[i].guid,
                    textures = ParseTextures(configs[i]),
                    meshes = ParseMeshes(configs[i]),
                };

                races.Add(raceMap);

                configs[i].prefabPath.addressPath = raceMap.prefabPath;
                configs[i].previewPrefabPath.addressPath = raceMap.previewPrefabPath;
                configs[i].createGamePrefabPath.addressPath = raceMap.createGamePrefabPath;
                EditorUtility.CopySerialized(configs[i], configs[i]);
            }

            return races;
        }

        private static List<GuidPathMap> ParseDataEntitiesPath<T>(DataLoader<T> loader, string group) where T: Object, IData
        {
            var pathMaps = new List<GuidPathMap>();
            var data = loader.LoadData();
            foreach (var entityData in data.Values)
            {
                var entityPath = AssetDatabase.GetAssetPath(entityData);
                var assetPath = SetupAddressable(entityPath, group);

                var map = new GuidPathMap {path = assetPath, guid = entityData.Guid};
                pathMaps.Add(map);
            }

            return pathMaps;
        }


        private static void ClearAddressables()
        {
            ClearAddressableGroup(CONFIG_GROUP_NAME);
            ClearAddressableGroup(PREFABS_GROUP_NAME);
        }

        private static string SetupAddressable(Object asset, string groupName)
        {
            var assetPath = AssetDatabase.GetAssetPath(asset);
            return SetupAddressable(assetPath, groupName);
        }

        private static string SetupAddressable(string assetPath, string groupName)
        {
            return SetupAddressable(assetPath, groupName, null, AssetsConstants.CharacterEditorRootPath,  AssetsConstants.GameRootPath);
        }

        private static string SetupAddressable(string assetPath, string groupName, string addressPath, params string[] addressableSkipFolders)
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            var addressableGroup = settings.FindGroup(groupName);
            if (!addressableGroup)
                addressableGroup = settings.CreateGroup(groupName, false, false, true, null,
                    typeof(ContentUpdateGroupSchema), typeof(BundledAssetGroupSchema));

            var entriesAdded = new List<AddressableAssetEntry>();
            var guid = AssetDatabase.AssetPathToGUID(assetPath);

            foreach (var skipFolder in addressableSkipFolders)
            {
                if (assetPath.IndexOf(skipFolder, StringComparison.Ordinal) == 0)
                    assetPath = assetPath.Substring(skipFolder.Length);
            }
          
            var addressableAddress = assetPath.Trim('/');
            var entry = settings.CreateOrMoveEntry(guid, addressableGroup, readOnly: false, postEvent: false);
            entry.address = !string.IsNullOrEmpty(addressPath) ? addressPath : addressableAddress;
            entriesAdded.Add(entry);

            settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, entriesAdded, true);
            return addressableAddress;
        }

        private static string GetAddressablePath(string assetPath)
        {
            var guid = AssetDatabase.AssetPathToGUID(assetPath);
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            var entry = settings.FindAssetEntry(guid);

            return entry?.address;
        }

        private static void ClearAddressableGroup(string groupName)
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            var addressableGroup = settings.FindGroup(groupName);
            if (addressableGroup == null) return;

            foreach (AddressableAssetEntry oldEntry in addressableGroup.entries.ToArray())
                addressableGroup.RemoveAssetEntry(oldEntry);
        }

        private static List<TexturesMap> ParseTextures(CharacterConfig characterConfig)
        {
            var dataManager = new DataManager(MeshAtlasType.Static);

            var mapTextures = new List<TexturesMap>();
            var textureEnums = Enum.GetValues(typeof(TextureType));
            var textureTypesCount = textureEnums.Length;
            for (int i = 0; i < textureTypesCount; i++)
            {
                var textureType = (TextureType) textureEnums.GetValue(i);
                if (textureType == TextureType.Undefined) continue;

                DisplayProgressBar("ParseTextures", $"{characterConfig.folderName} {textureType}", (float) i / textureTypesCount);
                var textureTypeGroup = $"{TEXTURE_GROUP_NAME}_{characterConfig.folderName}_{textureType}";

                var textures = new TexturesMap();
                textures.type = textureType;
                var paths = dataManager.ParseCharacterTextures(characterConfig, textureType);
                if (paths == null) continue;

                foreach (string[] texturePaths in paths)
                {
                    var texture = new MapTexture();
                    texture.colorPaths = new string[texturePaths.Length];
                    var colorIndex = 0;
                    foreach (string colorPath in texturePaths)
                    {
                        var assetPath = SetupAddressable(colorPath, textureTypeGroup);
                        texture.colorPaths[colorIndex++] = assetPath;
                    }

                    textures.texturePaths.Add(texture);
                }

                mapTextures.Add(textures);

            }

            return mapTextures;
        }

        private static List<MeshesMap> ParseMeshes(CharacterConfig characterConfig)
        {
            var dataManager = new DataManager(MeshAtlasType.Static);
            var mapMeshes = new List<MeshesMap>();

            var meshTypes = Enum.GetValues(typeof(MeshType));
            var meshTypesCount = meshTypes.Length;
            for (int i = 0; i < meshTypesCount; i++)
            {
                var meshType = (MeshType) meshTypes.GetValue(i);
                if (meshType == MeshType.Undefined) continue;

                DisplayProgressBar("ParseMeshes", $"{characterConfig.folderName} {meshType}", (float) i / meshTypesCount);

                var meshesMap = new MeshesMap();
                meshesMap.type = meshType;

                var meshPaths = dataManager.ParseCharacterMeshes(characterConfig, meshType);
                if (meshPaths == null || meshPaths.Count == 0) continue;

                var addressableGroup = $"{MESH_GROUP_NAME}_{characterConfig.folderName}_{meshType}";
                foreach (var meshPathPair in meshPaths)
                {
                    var meshPath = meshPathPair.Key;
                    var meshAddressablePath = SetupAddressable(meshPath, addressableGroup);

                    var mesh = new MapMesh();
                    mesh.modelPath = meshAddressablePath;

                    foreach (var texturePath in meshPathPair.Value)
                    {
                        var textures = new MapTexture();
                        textures.colorPaths = new string[texturePath.Length];
                        var colorIndex = 0;
                        foreach (var colorPath in texturePath)
                        {
                            var textureAddressablePath = SetupAddressable(colorPath, addressableGroup);
                            textures.colorPaths[colorIndex++] = textureAddressablePath;
                        }

                        mesh.textures.Add(textures);
                    }

                    meshesMap.meshPaths.Add(mesh);
                }

                mapMeshes.Add(meshesMap);
            }

            return mapMeshes;
        }


        private static void DisplayProgressBar(string title, string info, float progress)
        {
            EditorUtility.DisplayCancelableProgressBar($"Update Addressable: {title}", info, progress);
        }
    }
}