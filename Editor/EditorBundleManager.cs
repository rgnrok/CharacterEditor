﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using CharacterEditor;
using CharacterEditor.AssetDatabaseLoader;
using CharacterEditor.CharacterInventory;
using CharacterEditor.JSONMap;
using UnityEditor;
using UnityEngine;


public class EditorBundleManager
{
    private const string GAME_DATA_PATH = "Assets/Game/Data/";
    private static readonly string ROOT_PATH = $"{AssetsConstants.CharacterEditorRootPath}/";
    private static readonly string PREFAB_PATH_PREFIX = $"{AssetsConstants.CharacterEditorRootPath}/Prefabs/";
    private static readonly string TEXTURE_PATH_PREFIX = $"{AssetsConstants.CharacterEditorRootPath}/Textures/Character";

    [MenuItem("Tools/Character Editor/Update AssetsBundle")]
    public static void UpdateAssetsBundle()
    {
        var loader = new ConfigLoader();
        var configs = loader.LoadConfigs().Result;
        UpdateBundles(configs);
    }

    private static void UpdateBundles(CharacterConfig[] configs)
    {
        var bundleMap = new BundleMap();

        DisplayProgressBar("Start", "", 0f);

        bundleMap.races = ParseConfigs(configs);

        DisplayProgressBar("Parse Items", "", 0.3f);
        bundleMap.items = ParseItemPath();
        
        DisplayProgressBar("Parse players", "", 0.4f);
        bundleMap.playerCharacters = ParsePlayerCharacterPath();
        
        DisplayProgressBar("Parse enemies", "", 0.8f);
        bundleMap.enemies = ParseEnemiesPath();
        
        DisplayProgressBar("Parse containers", "", 0.9f);
        bundleMap.containers = ParseContainersPath();

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

        EditorUtility.ClearProgressBar();
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
                prefabPath = ParsePrefabPath(configs[i].prefabPath),
                previewPrefabPath = ParsePrefabPath(configs[i].previewPrefabPath),
                createGamePrefabPath = ParsePrefabPath(configs[i].createGamePrefabPath),
                enemyPrefabPath = ParsePrefabPath(configs[i].enemyPrefabPath),
                configGuid = configs[i].guid,
                textures = ParseBundleTextures(raceName),
                meshes = ParseBundleMeshes(raceName),
            };

            races.Add(raceMap);

            configs[i].bundlePrefabPath = raceMap.prefabPath;
            configs[i].previewBundlePrefabPath = raceMap.previewPrefabPath;
            configs[i].createGameBundlePrefabPath = raceMap.createGamePrefabPath;
            configs[i].enemyBundlePrefabPath = raceMap.enemyPrefabPath;
            EditorUtility.CopySerialized(configs[i], configs[i]);
        }

        return races;
    }


    private static List<GuidPathMap> ParseItemPath()
    {
        var itemMaps = new List<GuidPathMap>();

        var loader = new ItemLoader();
        loader.LoadData(items =>
        {
            foreach (var itemData in items.Values)
            {
                var itemPath = AssetDatabase.GetAssetPath(itemData);
                Debug.Log("itemPath " + itemPath);
                var bundlePath = itemPath.Substring(GAME_DATA_PATH.Length);
                ParsePathToBundle(bundlePath, out var bundleName, out var assetPath);

                AssetImporter.GetAtPath(itemPath).SetAssetBundleNameAndVariant(bundleName, "");
                var map = new GuidPathMap {path = assetPath, guid = itemData.guid};
                itemMaps.Add(map);

                UpdateItemPrefabBundlePath(itemData);

                var equipItemData = itemData as EquipItemData;
                if (equipItemData != null)
                    UpdateEquipItemPrefabBundlePath(equipItemData);
            }
        });

        return itemMaps;
    }

    private static void UpdateItemPrefabBundlePath(ItemData itemData)
    {
        if (itemData.prefabPath == null || itemData.prefabPath.Equals("")) return;

        var length = 0;
        if (itemData.prefabPath.IndexOf(PREFAB_PATH_PREFIX, StringComparison.Ordinal) != -1)
            length = PREFAB_PATH_PREFIX.Length;

        ParsePathToBundle(itemData.prefabPath.Substring(length), out var prefabBundleName, out var prefabAssetPath, 2, "prefabObj");
        itemData.prefabBundlePath = prefabAssetPath;
        AssetImporter.GetAtPath(itemData.prefabPath).SetAssetBundleNameAndVariant(prefabBundleName, "");

        EditorUtility.CopySerialized(itemData, itemData);
        AssetDatabase.SaveAssets();
    }

    private static void UpdateEquipItemPrefabBundlePath(EquipItemData equipItemData)
    {
        if (equipItemData == null) return;

        var length = 0;
        for (int i = 0; i < equipItemData.configsItems.Length; i++)
        {
            // Update config guids 
            var config = AssetDatabase.LoadAssetAtPath<CharacterConfig>(equipItemData.configsItems[i].configPath);
            if (config != null) equipItemData.configsItems[i].configGuid = config.guid;

            //Convert Texture path
            for (int j = 0; j < equipItemData.configsItems[i].textures.Length; j++)
            {
                var textureInfo = equipItemData.configsItems[i].textures[j];
                if (textureInfo.texturePath != null && !textureInfo.texturePath.Equals(""))
                {
                    if (textureInfo.texturePath.IndexOf(TEXTURE_PATH_PREFIX, StringComparison.Ordinal) != -1)
                        length = TEXTURE_PATH_PREFIX.Length;

                    ParsePathToBundle(textureInfo.texturePath.Substring(length), out var textureBundleName, out var textureAssetPath, 1);
                    textureInfo.textureBundlePath = textureAssetPath;
                }
            }

            for (int j = 0; j < equipItemData.configsItems[i].models.Length; j++)
            {
                var prefabAndTextureInfo = equipItemData.configsItems[i].models[j];

                // Main Texture (right hand)
                prefabAndTextureInfo.textureBundlePath  = GetEquipItemPrefabModelBundlePath(prefabAndTextureInfo.texturePath);
                // Additional Texture (left hand)
                prefabAndTextureInfo.additionalTextureBundlePath = GetEquipItemPrefabModelBundlePath(prefabAndTextureInfo.additionalTexturePath);

                //Convert Main Prefab path (right)
                prefabAndTextureInfo.prefabBundlePath = GetEquipItemPrefabModelBundlePath(prefabAndTextureInfo.prefabPath); 
                //Convert Additional Prefab path (left)
                prefabAndTextureInfo.additionalPrefabBundlePath = GetEquipItemPrefabModelBundlePath(prefabAndTextureInfo.additionalPrefabPath);
            }
        }

        EditorUtility.CopySerialized(equipItemData, equipItemData);
        AssetDatabase.SaveAssets();
    }

    private static string GetEquipItemPrefabModelBundlePath(string prefabPath)
    {
        if (string.IsNullOrEmpty(prefabPath)) return null;

        var length = 0;
        if (prefabPath.IndexOf(PREFAB_PATH_PREFIX, StringComparison.Ordinal) != -1)
            length = PREFAB_PATH_PREFIX.Length;

        ParsePathToBundle(prefabPath.Substring(length), out var prefabBundleName, out var prefabAssetPath, 2);
        return prefabAssetPath;

    }

    protected static List<GuidPathMap> ParsePlayerCharacterPath()
    {
        var characterMap = new List<GuidPathMap>();
        var loader = new PlayerCharacterLoader();
        loader.LoadData(characters =>
        {
            foreach (var character in characters.Values)
            {
                var characterPath = AssetDatabase.GetAssetPath(character);

                var bundlePath = characterPath.Substring(GAME_DATA_PATH.Length);
                ParsePathToBundle(bundlePath, out var bundleName, out var assetPath);

                AssetImporter.GetAtPath(characterPath).SetAssetBundleNameAndVariant(bundleName, "");
                var map = new GuidPathMap();
                map.path = assetPath;
                map.guid = character.guid;
                characterMap.Add(map);

                var texturesBundleName = string.Format("{0}_{1}", bundleName, character.guid);
                AssetImporter.GetAtPath(character.texturePath).SetAssetBundleNameAndVariant(texturesBundleName, "");
                AssetImporter.GetAtPath(character.faceMeshTexturePath).SetAssetBundleNameAndVariant(texturesBundleName, "");
                character.textureBundlePath = GenerateAssetPath(texturesBundleName, character.texturePath);
                character.faceMeshTextureBundlePath = GenerateAssetPath(texturesBundleName, character.faceMeshTexturePath);

                for (int i = 0; i < character.faceMeshs.Length; i++)
                {
                    var faceMesh = character.faceMeshs[i];
                    var meshBundleName = AssetDatabase.GetImplicitAssetBundleName(faceMesh.meshPath);

                    faceMesh.meshBundlePath = GenerateAssetPath(meshBundleName, faceMesh.meshPath);
                }

                EditorUtility.CopySerialized(character, character);
                AssetDatabase.SaveAssets();
            }
        });

        return characterMap;
    }

    private static List<GuidPathMap> ParseEnemiesPath()
    {
        string bundleName, assetPath;

        var enemyMap = new List<GuidPathMap>();
        var loader = new EnemyLoader();
        loader.LoadData(enemies =>
        {
            foreach (var enemy in enemies.Values)
            {
                var enemyPath = AssetDatabase.GetAssetPath(enemy);
                var prefabBonePath = AssetDatabase.GetAssetPath(enemy.prefabBoneData);

                var bundlePath = enemyPath.Substring(GAME_DATA_PATH.Length);
                ParsePathToBundle(bundlePath, out bundleName, out assetPath);

                AssetImporter.GetAtPath(enemyPath).SetAssetBundleNameAndVariant(bundleName, "");
                var map = new GuidPathMap
                {
                    path = assetPath,
                    guid = enemy.guid
                };
                enemyMap.Add(map);

                var texturesBundleName = string.Format("{0}_{1}", bundleName, enemy.guid);
                AssetImporter.GetAtPath(enemy.texturePath).SetAssetBundleNameAndVariant(texturesBundleName, "");
                AssetImporter.GetAtPath(enemy.faceMeshTexturePath).SetAssetBundleNameAndVariant(texturesBundleName, "");
                AssetImporter.GetAtPath(enemy.armorTexturePath).SetAssetBundleNameAndVariant(texturesBundleName, "");
                AssetImporter.GetAtPath(prefabBonePath).SetAssetBundleNameAndVariant(texturesBundleName, "");
                AssetImporter.GetAtPath(enemy.materialPath).SetAssetBundleNameAndVariant(texturesBundleName, "");

                enemy.textureBundlePath = GenerateAssetPath(texturesBundleName, enemy.texturePath);
                enemy.faceMeshTextureBundlePath = GenerateAssetPath(texturesBundleName, enemy.faceMeshTexturePath);
                enemy.armorTextureBundlePath = GenerateAssetPath(texturesBundleName, enemy.armorTexturePath);
                enemy.materialBundlePath = GenerateAssetPath(texturesBundleName, enemy.materialPath);

                EditorUtility.CopySerialized(enemy, enemy);
                AssetDatabase.SaveAssets();
            }
        });

        return enemyMap;
    }

    private static List<GuidPathMap> ParseContainersPath()
    {
        string bundleName, assetPath;

        var containerMap = new List<GuidPathMap>();
        var loader = new ContainerLoader();
        loader.LoadData(data =>
        {
            foreach (var container in data.Values)
            {
                var containerPath = AssetDatabase.GetAssetPath(container);

                var bundlePath = containerPath.Substring(GAME_DATA_PATH.Length);
                ParsePathToBundle(bundlePath, out bundleName, out assetPath);

                AssetImporter.GetAtPath(containerPath).SetAssetBundleNameAndVariant(bundleName, "");
                var map = new GuidPathMap
                {
                    path = assetPath,
                    guid = container.guid
                };
                containerMap.Add(map);

                EditorUtility.CopySerialized(container, container);
                AssetDatabase.SaveAssets();
            }
        });

        return containerMap;
    }

    private static string ParseConfigPath(CharacterConfig config)
    {
        var configPath = AssetDatabase.GetAssetPath(config);

        var bundlePath = configPath.Substring(ROOT_PATH.Length);
        ParsePathToBundle(bundlePath, out var bundleName, out var assetPath);

        AssetImporter.GetAtPath(configPath).SetAssetBundleNameAndVariant(bundleName, "");
        return assetPath;
    }

    private static string ParsePrefabPath(string prefabPath)
    {
        if (string.IsNullOrEmpty(prefabPath)) return prefabPath;

        var bundlePath = prefabPath.Substring(ROOT_PATH.Length);
        ParsePathToBundle(bundlePath, out var bundleName, out var assetPath);

        AssetImporter.GetAtPath(prefabPath).SetAssetBundleNameAndVariant(bundleName, "");
        return assetPath;
    }

    private static List<MeshesMap> ParseBundleMeshes(string raceName)
    {
        var meshLoader = new MeshLoader(new TextureLoader(), MeshAtlasType.Static);
        var bundleMeshList = new List<MeshesMap>();

        int substringLength = PREFAB_PATH_PREFIX.Length;

        foreach (MeshType meshType in Enum.GetValues(typeof(MeshType)))
        {
            var bundleMeshes = new MeshesMap();
            bundleMeshes.type = meshType;

            var meshPaths = meshLoader.ParseMeshes(raceName, meshType);
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

    protected static string FixPrefabName(string oldName)
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

    protected static List<TexturesMap> ParseBundleTextures(string raceName)
    {
        var loader = new TextureLoader();

        var bundleTextures = new List<TexturesMap>();
        foreach (TextureType textureType in Enum.GetValues(typeof(TextureType)))
        {
            var textures = new TexturesMap();
            textures.type = textureType;
            var paths = loader.ParseCharacterTextures(raceName, textureType);
            if (paths == null) continue;

            foreach (string[] texturePaths in paths)
            {
                var texture = new MapTexture();
                texture.colorPaths = new string[texturePaths.Length];
                int i = 0;
                foreach (var colorPath in texturePaths)
                {
                    var bundlePath = colorPath.Substring(TEXTURE_PATH_PREFIX.Length);
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

    private static void ParsePathToBundle(string bundlePath, out string bundleName, out string assetPath, int rootDepth = 1, string bundleNamePostfix = null)
    {
        var builder = new StringBuilder();

        bundlePath = bundlePath.Replace(' ', '_');
        var pathParts = bundlePath.Split('/');
        builder.Length = 0;
        for (int j = 0; j < pathParts.Length - rootDepth; j++)
            builder.Append(pathParts[j]).Append("_");

        if (bundleNamePostfix != null) builder.Append(bundleNamePostfix);
        bundleName = builder.ToString().Trim('_');

        var assetNameParts = pathParts[pathParts.Length - 1].Split('.');
        builder.Length = 0;
        builder.Append(bundleName).Append("/");

        for (int j = 0; j < assetNameParts.Length - 1; j++)
            builder.Append(assetNameParts[j]);
        
        assetPath = builder.ToString();
    }

    private static string GenerateAssetPath(string bundleName, string path)
    {
        var pathParts = path.Split('/');
        var assetNameParts = pathParts[pathParts.Length - 1].Split('.');

        var builder = new StringBuilder();
        builder.Append(bundleName).Append("/");

        for (int j = 0; j < assetNameParts.Length - 1; j++)
            builder.Append(assetNameParts[j]);
        
        return builder.ToString();

    }

    private static void DisplayProgressBar(string title, string info, float progress)
    {
        EditorUtility.DisplayCancelableProgressBar($"Update bundles: {title}", info, progress);
    }
}
