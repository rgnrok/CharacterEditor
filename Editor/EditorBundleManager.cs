using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using CharacterEditor;
using CharacterEditor.AssetDatabaseLoader;
using CharacterEditor.CharacterInventory;
using CharacterEditor.JSONMap;
using CharacterEditor.Services;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Editor
{
    public class EditorBundleManager
    {
        private const string GAME_DATA_PATH = "Assets/Game/Data/";
        private static readonly string PrefabPathPrefix = $"{AssetsConstants.CharacterEditorRootPath}/Prefabs/";

        private static readonly string TexturePathPrefix =
            $"{AssetsConstants.CharacterEditorRootPath}/Textures/Character";

        [MenuItem("Tools/Character Editor/Update AssetsBundle")]
        public static void UpdateAssetsBundle()
        {
            var loader = new ConfigLoader();
            var configs = loader.LoadConfigs().Result;
            try
            {
                UpdateBundles(configs);
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

        private static void UpdateBundles(CharacterConfig[] configs)
        {
            var bundleMap = new DataMap();

            DisplayProgressBar("Start", "", 0f);

            bundleMap.races = ParseConfigs(configs);

            DisplayProgressBar("Parse Items", "", 0.3f);
            bundleMap.items = ParseItems();

            DisplayProgressBar("Parse npc", "", 0.4f);
            bundleMap.playableNpc = ParsePlayableNpc();

            DisplayProgressBar("Parse enemies", "", 0.7f);
            bundleMap.enemies = ParseEnemies();

            DisplayProgressBar("Parse containers", "", 0.8f);
            bundleMap.containers = ParseContainers();

            DisplayProgressBar("Parse materials", "", 0.9f);
            bundleMap.materials = ParseMaterials();


            if (!Directory.Exists(Application.dataPath + "/Resources/"))
                Directory.CreateDirectory(Application.dataPath + "/Resources/");

            using (var fs = new FileStream("Assets/Resources/assetBundleInfo.json", FileMode.Create))
            {
                using (var writer = new StreamWriter(fs))
                {
                    writer.Write(JsonUtility.ToJson(bundleMap));
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static List<RaceMap> ParseConfigs(CharacterConfig[] configs)
        {
            var configsCount = configs.Length;
            var races = new List<RaceMap>(configsCount);

            for (int i = 0; i < configsCount; i++)
            {
                var raceName = configs[i].folderName;
                DisplayProgressBar("ParseRacesConfigs", raceName, (float) i / configsCount);

                var raceMap = new RaceMap
                {
                    race = raceName,
                    configPath = ParseConfigPath(configs[i]),
                    prefabPath = ParsePrefabPath(configs[i].prefabPath.path),
                    previewPrefabPath = ParsePrefabPath(configs[i].previewPrefabPath.path),
                    createGamePrefabPath = ParsePrefabPath(configs[i].createGamePrefabPath.path),
                    configGuid = configs[i].guid,
                    textures = ParseBundleTextures(configs[i]),
                    meshes = ParseBundleMeshes(configs[i]),
                };

                races.Add(raceMap);

                configs[i].prefabPath.bundlePath = raceMap.prefabPath;
                configs[i].previewPrefabPath.bundlePath = raceMap.previewPrefabPath;
                configs[i].createGamePrefabPath.bundlePath = raceMap.createGamePrefabPath;
                EditorUtility.CopySerialized(configs[i], configs[i]);
            }

            return races;
        }


        private static List<GuidPathMap> ParseDataEntities<T>(DataLoader<T> loader) where T: Object, IData
        {
            var pathMaps = new List<GuidPathMap>();

            var data = loader.LoadData();
            foreach (var itemData in data.Values)
            {
                var itemPath = AssetDatabase.GetAssetPath(itemData);
     
                var bundlePath = itemPath.Substring(GAME_DATA_PATH.Length);
                ParsePathToBundle(bundlePath, out var bundleName, out var assetPath);
                AssetImporter.GetAtPath(itemPath).SetAssetBundleNameAndVariant(bundleName, "");

                var map = new GuidPathMap {path = assetPath, guid = itemData.Guid};
                pathMaps.Add(map);
            }

            return pathMaps;
        }

        private static List<GuidPathMap> ParseItems()
        {
            var loader = new ItemLoader();
            var items = loader.LoadData();
            foreach (var itemData in items.Values)
            {
                UpdateItemDataBundlePaths(itemData);

                if (itemData is EquipItemData equipItemData)
                    UpdateEquipItemPrefabBundlePath(equipItemData);
            }
            AssetDatabase.SaveAssets();

            return ParseDataEntities(loader);
        }

        private static void UpdateItemDataBundlePaths(ItemData itemData)
        {
            if (string.IsNullOrEmpty(itemData.prefab.path)) return;

            var prefabPath = itemData.prefab.path;
            if (prefabPath.IndexOf(PrefabPathPrefix, StringComparison.Ordinal) == 0)
                prefabPath = prefabPath.Substring(PrefabPathPrefix.Length);

            ParsePathToBundle(prefabPath, out var prefabBundleName, out var prefabAssetPath, 2);
            AssetImporter.GetAtPath(itemData.prefab.path).SetAssetBundleNameAndVariant(prefabBundleName, "");

            itemData.prefab.bundlePath = prefabAssetPath;
            EditorUtility.CopySerialized(itemData, itemData);
        }

        private static void UpdateEquipItemPrefabBundlePath(EquipItemData equipItemData)
        {
            foreach (var equipItem in equipItemData.configsItems)
            {
                foreach (var textureInfo in equipItem.textures)
                    textureInfo.texture.bundlePath = GetModelBundlePath(textureInfo.texture.path, TexturePathPrefix);

                foreach (var prefabAndTextureInfo in equipItem.models)
                {
                    // Main Texture (right hand)
                    prefabAndTextureInfo.texture.bundlePath =
                        GetModelBundlePath(prefabAndTextureInfo.texture.path, PrefabPathPrefix, 2);
                    // Additional Texture (left hand)
                    prefabAndTextureInfo.additionalTexture.bundlePath =
                        GetModelBundlePath(prefabAndTextureInfo.additionalTexture.path, PrefabPathPrefix, 2);

                    //Convert Main Prefab path (right)
                    prefabAndTextureInfo.prefab.bundlePath =
                        GetModelBundlePath(prefabAndTextureInfo.prefab.path, PrefabPathPrefix, 2);
                    //Convert Additional Prefab path (left)
                    prefabAndTextureInfo.additionalPrefab.bundlePath =
                        GetModelBundlePath(prefabAndTextureInfo.additionalPrefab.path, PrefabPathPrefix, 2);
                }
            }

            EditorUtility.CopySerialized(equipItemData, equipItemData);
        }

        private static string GetModelBundlePath(string prefabPath, string prefix, int depth = 1)
        {
            if (string.IsNullOrEmpty(prefabPath)) return null;

            if (prefabPath.IndexOf(prefix, StringComparison.Ordinal) == 0)
                prefabPath = prefabPath.Substring(prefix.Length);

            ParsePathToBundle(prefabPath, out var prefabBundleName, out var prefabAssetPath, depth);
            return prefabAssetPath;

        }

        private static List<GuidPathMap> ParsePlayableNpc()
        {
            var npcMap = new List<GuidPathMap>();
            var loader = new PlayableNpcLoader();

            var npcs = loader.LoadData();
            foreach (var npcData in npcs.Values)
            {
                var characterPath = AssetDatabase.GetAssetPath(npcData);
                var bundlePath = characterPath.Substring(GAME_DATA_PATH.Length);
                ParsePathToBundle(bundlePath, out var bundleName, out var assetPath);
                
                AssetImporter.GetAtPath(characterPath).SetAssetBundleNameAndVariant(bundleName, "");
                var map = new GuidPathMap {path = assetPath, guid = npcData.guid};
                npcMap.Add(map);

                AssetImporter.GetAtPath(npcData.texturePath.path).SetAssetBundleNameAndVariant(bundleName, "");
                AssetImporter.GetAtPath(npcData.faceMeshTexturePath.path).SetAssetBundleNameAndVariant(bundleName, "");

                npcData.texturePath.bundlePath = GenerateAssetBundlePath(bundleName, npcData.texturePath.path);
                npcData.faceMeshTexturePath.bundlePath = GenerateAssetBundlePath(bundleName, npcData.faceMeshTexturePath.path);

                foreach (var faceMesh in npcData.faceMeshs)
                {
                    var meshBundleName = AssetDatabase.GetImplicitAssetBundleName(faceMesh.meshPath.path);
                    faceMesh.meshPath.bundlePath = GenerateAssetBundlePath(meshBundleName, faceMesh.meshPath.path);
                }

                EditorUtility.CopySerialized(npcData, npcData);
            }
            AssetDatabase.SaveAssets();

            return npcMap;
        }

        private static List<GuidPathMap> ParseEnemies()
        {
            var enemyMap = new List<GuidPathMap>();
            var loader = new EnemyLoader();
            var enemies = loader.LoadData();
            foreach (var enemy in enemies.Values)
            {
                var enemyPath = AssetDatabase.GetAssetPath(enemy);
                var prefabBonePath = AssetDatabase.GetAssetPath(enemy.prefabBoneData);

                var bundlePath = enemyPath.Substring(GAME_DATA_PATH.Length);
                ParsePathToBundle(bundlePath, out var bundleName, out var assetPath);

                AssetImporter.GetAtPath(enemyPath).SetAssetBundleNameAndVariant(bundleName, "");
                var map = new GuidPathMap
                {
                    path = assetPath,
                    guid = enemy.guid
                };
                enemyMap.Add(map);

                var texturesBundleName = $"{bundleName}_{enemy.guid}";
                AssetImporter.GetAtPath(enemy.texturePath.path).SetAssetBundleNameAndVariant(texturesBundleName, "");
                AssetImporter.GetAtPath(enemy.faceMeshTexturePath.path).SetAssetBundleNameAndVariant(texturesBundleName, "");
                AssetImporter.GetAtPath(enemy.armorTexturePath.path).SetAssetBundleNameAndVariant(texturesBundleName, "");
                AssetImporter.GetAtPath(prefabBonePath).SetAssetBundleNameAndVariant(texturesBundleName, "");
                AssetImporter.GetAtPath(enemy.materialPath.path).SetAssetBundleNameAndVariant(texturesBundleName, "");

                enemy.texturePath.bundlePath = GenerateAssetBundlePath(texturesBundleName, enemy.texturePath.path);
                enemy.faceMeshTexturePath.bundlePath = GenerateAssetBundlePath(texturesBundleName, enemy.faceMeshTexturePath.path);
                enemy.armorTexturePath.bundlePath = GenerateAssetBundlePath(texturesBundleName, enemy.armorTexturePath.path);
                enemy.materialPath.bundlePath = GenerateAssetBundlePath(texturesBundleName, enemy.materialPath.path);
                
                enemy.prefabPath.bundlePath = ParsePrefabPath(enemy.prefabPath.path);

                EditorUtility.CopySerialized(enemy, enemy);
            }
            AssetDatabase.SaveAssets();

            return enemyMap;
        }

        //todo add prefab path in feature
        private static List<GuidPathMap> ParseContainers()
        {
            var loader = new ContainerLoader();
            return ParseDataEntities(loader);
        }

        private static List<GuidPathMap> ParseMaterials()
        {
            var staticData = new StaticDataService();
            staticData.LoadData();

            var armorMatPath = AssetDatabase.GetAssetPath(staticData.GameData.ArmorMergeMaterial);
            var clothMatPath = AssetDatabase.GetAssetPath(staticData.GameData.ClothMergeMaterial);
            var modelMatPath = AssetDatabase.GetAssetPath(staticData.GameData.ModelMaterial);
            var previewMatPath = AssetDatabase.GetAssetPath(staticData.GameData.PreviewMaterial);

            ParsePathToBundle(armorMatPath, out var bundleName, out var armorMatGuidPath);
            AssetImporter.GetAtPath(armorMatPath).SetAssetBundleNameAndVariant(bundleName, "");

            ParsePathToBundle(clothMatPath, out bundleName, out var clothMatGuidPath);
            AssetImporter.GetAtPath(clothMatPath).SetAssetBundleNameAndVariant(bundleName, "");

            ParsePathToBundle(modelMatPath, out bundleName, out var modelMatGuidPath);
            AssetImporter.GetAtPath(modelMatPath).SetAssetBundleNameAndVariant(bundleName, "");

            ParsePathToBundle(previewMatPath, out bundleName, out var previewMatGuidPath);
            AssetImporter.GetAtPath(previewMatPath).SetAssetBundleNameAndVariant(bundleName, "");

            var map = new List<GuidPathMap>()
            {
                new GuidPathMap {guid = AssetsConstants.ArmorMergeMaterialPathKey, path = armorMatGuidPath},
                new GuidPathMap {guid = AssetsConstants.ClothMergeMaterialPathKey, path = clothMatGuidPath},
                new GuidPathMap {guid = AssetsConstants.ModelMaterialPathKey, path = modelMatGuidPath},
                new GuidPathMap {guid = AssetsConstants.PreviewMaterialPathKey, path = previewMatGuidPath},
            };

            return map;
        }

        private static string ParseConfigPath(CharacterConfig config)
        {
            var configPath = AssetDatabase.GetAssetPath(config);

            var bundlePath = configPath.Substring(AssetsConstants.CharacterEditorRootPath.Length).Trim('/');
            ParsePathToBundle(bundlePath, out var bundleName, out var assetPath);

            AssetImporter.GetAtPath(configPath).SetAssetBundleNameAndVariant(bundleName, "");
            return assetPath;
        }

        private static string ParsePrefabPath(string prefabPath)
        {
            if (string.IsNullOrEmpty(prefabPath)) return prefabPath;

            var bundlePath = prefabPath;
            if (prefabPath.IndexOf(AssetsConstants.CharacterEditorRootPath, StringComparison.Ordinal) == 0)
                bundlePath = prefabPath.Substring(AssetsConstants.CharacterEditorRootPath.Length).Trim('/');
            if (prefabPath.IndexOf(AssetsConstants.GameRootPath, StringComparison.Ordinal) == 0)
                bundlePath = prefabPath.Substring(AssetsConstants.GameRootPath.Length).Trim('/');

            ParsePathToBundle(bundlePath, out var bundleName, out var assetPath);

            AssetImporter.GetAtPath(prefabPath).SetAssetBundleNameAndVariant(bundleName, "");
            return assetPath;
        }

        private static List<MeshesMap> ParseBundleMeshes(CharacterConfig characterConfig)
        {
            var dataManager = new DataManager(MeshAtlasType.Static);
            var bundleMeshList = new List<MeshesMap>();

            int substringLength = PrefabPathPrefix.Length;

            foreach (MeshType meshType in Enum.GetValues(typeof(MeshType)))
            {
                var bundleMeshes = new MeshesMap();
                bundleMeshes.type = meshType;

                var meshPaths = dataManager.ParseCharacterMeshes(characterConfig, meshType);
                if (meshPaths == null || meshPaths.Count == 0) continue;

                foreach (var meshPathPair in meshPaths)
                {
                    var bundleMesh = new MapMesh();
                    var meshPath = meshPathPair.Key;

                    // Fix prefab path
                    meshPath = FixPrefabName(meshPath);

                    ParsePathToBundle(meshPath.Substring(substringLength), out var bundleName, out var assetPath, 2);
                    AssetImporter.GetAtPath(meshPath).SetAssetBundleNameAndVariant(bundleName, "");

                    bundleMesh.modelPath = assetPath;

                    foreach (var texturePath in meshPathPair.Value)
                    {
                        var bundleTexture = new MapTexture();

                        bundleTexture.colorPaths = new string[texturePath.Length];
                        var i = 0;
                        foreach (var colorPath in texturePath)
                        {
                            ParsePathToBundle(colorPath.Substring(substringLength), out bundleName, out assetPath, 2);
                            AssetImporter.GetAtPath(colorPath).SetAssetBundleNameAndVariant(bundleName, "");

                            bundleTexture.colorPaths[i++] = assetPath;
                        }

                        bundleMesh.textures.Add(bundleTexture);
                    }

                    bundleMeshes.meshPaths.Add(bundleMesh);
                }

                bundleMeshList.Add(bundleMeshes);
            }

            return bundleMeshList;
        }

        private static string FixPrefabName(string oldName)
        {
            var pathParts = oldName.Split('/');
            if (pathParts.Length == 0 || pathParts[pathParts.Length - 1].IndexOf(' ') == -1) return oldName;

            var prefabName = pathParts[pathParts.Length - 1].Replace(' ', '_');

            var builder = new StringBuilder();
            for (var pi = 0; pi < pathParts.Length - 1; pi++)
                builder.Append(pathParts[pi]).Append("/");

            builder.Append(prefabName);
            var newName = builder.ToString().Trim('_');
            AssetDatabase.RenameAsset(oldName, prefabName);

            return newName;
        }

        private static List<TexturesMap> ParseBundleTextures(CharacterConfig characterConfig)
        {
            var dataManager = new DataManager(MeshAtlasType.Static);

            var bundleTextures = new List<TexturesMap>();
            foreach (TextureType textureType in Enum.GetValues(typeof(TextureType)))
            {
                var textures = new TexturesMap();
                textures.type = textureType;
                var paths = dataManager.ParseCharacterTextures(characterConfig, textureType);
                if (paths == null) continue;

                foreach (var texturePaths in paths)
                {
                    var texture = new MapTexture();
                    texture.colorPaths = new string[texturePaths.Length];
                    int i = 0;
                    foreach (var colorPath in texturePaths)
                    {
                        var bundlePath = colorPath.Substring(TexturePathPrefix.Length);
                        ParsePathToBundle(bundlePath, out var bundleName, out var assetPath);
                        AssetImporter.GetAtPath(colorPath).SetAssetBundleNameAndVariant(bundleName, "");

                        texture.colorPaths[i++] = assetPath;
                    }

                    textures.texturePaths.Add(texture);
                }

                bundleTextures.Add(textures);

            }

            return bundleTextures;
        }

        private static void ParsePathToBundle(string bundlePath, out string bundleName, out string assetPath,
            int rootDepth = 1, string bundleNamePostfix = null)
        {
            var builder = new StringBuilder();

            bundlePath = bundlePath.Replace(' ', '_');
            var pathParts = bundlePath.Split('/');
            for (var i = 0; i < pathParts.Length - rootDepth; i++)
                builder.Append(pathParts[i]).Append("_");

            if (bundleNamePostfix != null) builder.Append(bundleNamePostfix);
            bundleName = builder.ToString().Trim('_');

            assetPath = GenerateAssetBundlePath(bundleName, pathParts[pathParts.Length - 1]);
        }

        private static string GenerateAssetBundlePath(string bundleName, string path)
        {
            var fileNameIndex = path.LastIndexOf('/');
            if (fileNameIndex != -1)
                path = path.Substring(fileNameIndex + 1);

            var extIndex = path.LastIndexOf('.');
            if (extIndex == -1)
                return $"{bundleName}/{path}";

            return $"{bundleName}/{path.Substring(0, extIndex)}";
        }

        private static void DisplayProgressBar(string title, string info, float progress)
        {
            EditorUtility.DisplayCancelableProgressBar($"Update bundles: {title}", info, progress);
        }
    }
}