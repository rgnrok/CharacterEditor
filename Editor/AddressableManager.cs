using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CharacterEditor;
using CharacterEditor.AssetDatabaseLoader;
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
        private const string NPC_GROUP_NAME = "PlayableNpc";
        private const string ENEMIES_GROUP_NAME = "Enemies";
        private const string CONTAINERS_GROUP_NAME = "Containers";

        [MenuItem("Tools/Character Editor/Refresh Addressables")]
        public static void RefreshAddressable()
        {
            var loader = new ConfigLoader();
            var configs = loader.LoadConfigs().Result;
            UpdateAddressables(configs);
        }


        private static void UpdateAddressables(CharacterConfig[] configs)
        {
            var bundleMap = new BundleMap();

            bundleMap.races = ParseRacesConfigs(configs);

            bundleMap.items = ParseDataEntitiesPath(new ItemLoader(), ITEMS_GROUP_NAME);
            bundleMap.playableNpc = ParseDataEntitiesPath(new PlayableNpcLoader(), NPC_GROUP_NAME);
            bundleMap.enemies = ParseDataEntitiesPath(new EnemyLoader(), ENEMIES_GROUP_NAME);
            bundleMap.containers = ParseDataEntitiesPath(new ContainerLoader(), CONTAINERS_GROUP_NAME);

            DisplayProgressBar("Save addressablesInfo", "", 0.9f);


            if (!Directory.Exists(Application.dataPath + "/Resources/"))
                Directory.CreateDirectory(Application.dataPath + "/Resources/");

            using (var fs = new FileStream("Assets/Resources/addressablesInfo.json", FileMode.Create))
            {
                using (var writer = new StreamWriter(fs))
                {
                    writer.Write(JsonUtility.ToJson(bundleMap));
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.ClearProgressBar();
        }

        private static List<RaceMap> ParseRacesConfigs(CharacterConfig[] configs)
        {
            ClearAddressables();

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
            var itemMaps = new List<GuidPathMap>();
            var data = loader.LoadData();
            {
                foreach (var entityData in data.Values)
                {
                    var entityPath = AssetDatabase.GetAssetPath(entityData);
                    var assetPath = SetupAddressable(entityPath, group);

                    var map = new GuidPathMap {path = assetPath, guid = entityData.Guid};
                    itemMaps.Add(map);
                }
            }
            return itemMaps;
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
            return SetupAddressable(assetPath, groupName, AssetsConstants.CharacterEditorRootPath, AssetsConstants.GameRootPath);
        }

        private static string SetupAddressable(string assetPath, string groupName, params string[] addressableSkipFolders)
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
            entry.address = addressableAddress;
            entriesAdded.Add(entry);

            settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, entriesAdded, true);
            return addressableAddress;
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

            var bundleTextures = new List<TexturesMap>();
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

                bundleTextures.Add(textures);

            }

            return bundleTextures;
        }

        private static List<MeshesMap> ParseMeshes(CharacterConfig characterConfig)
        {
            var dataManager = new DataManager(MeshAtlasType.Static);
            var bundleMeshList = new List<MeshesMap>();

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

                bundleMeshList.Add(meshesMap);
            }

            return bundleMeshList;
        }


        private static void DisplayProgressBar(string title, string info, float progress)
        {
            EditorUtility.DisplayCancelableProgressBar($"Update Addressable: {title}", info, progress);
        }
    }
}