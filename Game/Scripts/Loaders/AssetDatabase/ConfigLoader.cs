#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;

namespace CharacterEditor
{
    namespace AssetDatabaseLoader
    {
        /*
         * Parse and Load Configs (Only editor)
         */
        public class ConfigLoader : IConfigLoader
        {
            private static readonly string CONFIG_PATHS = $"{AssetsConstants.CharacterStaticDataPath}/CharacterConfigs";

            private readonly Dictionary<string, CharacterConfig> _configCache = new Dictionary<string, CharacterConfig>();

            public Task<CharacterConfig[]> LoadConfigs()
            {
                ParseConfigGuids();
                return Task.FromResult(_configCache.Values.ToArray());
            }

            public Task<CharacterConfig> LoadConfig(string guid)
            {
                _configCache.TryGetValue(guid, out var config);
                return Task.FromResult(config);
            }

            public void ParseConfigGuids()
            {
                var paths = AssetDatabase.FindAssets("t:CharacterConfig", new string[] { CONFIG_PATHS });
                for (var i = 0; i < paths.Length; i++)
                {
                    var config = AssetDatabase.LoadAssetAtPath<CharacterConfig>(AssetDatabase.GUIDToAssetPath(paths[i]));
                    config.Prefab = AssetDatabase.LoadAssetAtPath<GameObject>(config.prefabPath);
                    config.PreviewPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(config.previewPrefabPath);

                    _configCache[config.guid] = config;
                }
            }
        }
    }
}
#endif