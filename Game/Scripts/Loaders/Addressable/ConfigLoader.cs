using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using AssetBundles;
using UnityEngine;

namespace CharacterEditor
{
    namespace AddressableLoader
    {
        public class ConfigLoader : IConfigLoader
        {
            private readonly DataManager _dataManager;

            private readonly Dictionary<string, CharacterConfig> _configCache = new Dictionary<string, CharacterConfig>();

            public ConfigLoader(DataManager dataManager)
            {
                _dataManager = dataManager;
            }

            public async Task<CharacterConfig[]> LoadConfigs()
            {
                var configs = new List<CharacterConfig>();
                foreach (var configInfo in _dataManager.Races.Values)
                {
                    var config = await LoadConfigData(configInfo.configPath);

                    configs.Add(config);
                    _configCache[config.guid] = config;
                }
                return configs.ToArray();
            }

            public async Task<CharacterConfig> LoadConfig(string guid)
            {
                Logger.Log("Start Load Config");
                if (_configCache.TryGetValue(guid, out var config))
                    return config;

                if (!_dataManager.Races.TryGetValue(guid, out var raceMap))
                    return null;

                config = await LoadConfigData(raceMap.configPath);
                _configCache[config.guid] = config;
                return config;
            }
            

            private async Task<CharacterConfig> LoadConfigData(string configPath)
            {
                var pathParts = configPath.Split('/');
                var assetBundleName = pathParts[0].ToLower();
                var assetName = pathParts[pathParts.Length - 1];

                var request = AssetBundleManager.LoadAssetAsync(assetBundleName, assetName, typeof(CharacterConfig));
                if (request == null)
                {
                    Logger.LogError("Failed AssetBundleLoadAssetOperation on " + assetName + " from the AssetBundle " + assetBundleName + ".");
                    return null;
                }

                while (!request.IsDone()) await Task.Yield();
                var config = request.GetAsset<CharacterConfig>();

                // Remove Load from factory
                config.Prefab = await LoadPrefab(config.bundlePrefabPath);
                config.PreviewPrefab = await LoadPrefab(config.previewBundlePrefabPath);
                config.CreateGamePrefab = await LoadPrefab(config.createGameBundlePrefabPath);
                config.EnemyPrefab = await LoadPrefab(config.enemyBundlePrefabPath);

                return config;
            }

            private async Task<GameObject> LoadPrefab(string prefabPath)
            {
                var pathParts = prefabPath.Split('/');
                var assetBundleName = pathParts[0].ToLower();
                var assetName = pathParts[pathParts.Length - 1];

                var request = AssetBundleManager.LoadAssetAsync(assetBundleName, assetName, typeof(GameObject));
                if (request == null)
                {
                    Debug.LogError("Failed AssetBundleLoadAssetOperation on " + assetName + " from the AssetBundle " + assetBundleName + ".");
                    return null;
                }

                while (!request.IsDone()) await Task.Yield();
                return request.GetAsset<GameObject>();
            }
        }
    }
}
