#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace CharacterEditor
{
    namespace AssetDatabaseLoader
    {
        public class CommonLoader<T> : ICommonLoader<T> where T : UnityEngine.Object
        {
            private readonly Dictionary<string, T> _cache = new Dictionary<string, T>();

            public async Task<T> LoadByPath(string path)
            {
                return InnerLoadByPath(path);
            }

            public void LoadByPath(string path, Action<string, T> callback)
            {
                callback?.Invoke(path, InnerLoadByPath(path));
            }

            public void LoadByPath(List<string> paths, Action<Dictionary<string, T>> callback)
            {
                var dataItems = new Dictionary<string, T>();
                foreach (var path in paths)
                {
                    if (_cache.TryGetValue(path, out var meshObject))
                    {
                        dataItems[path] = meshObject;
                        continue;
                    }

                    var asset = AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(path));
                    if (asset == null) continue;

                    _cache[path] = asset;
                    dataItems[path] = asset;
                }
                callback.Invoke(dataItems);
            }

            public void Unload(string path)
            {
                Resources.UnloadUnusedAssets();
            }

            private T InnerLoadByPath(string path)
            {
                if (_cache.TryGetValue(path, out var asset))
                    return asset;

                asset = AssetDatabase.LoadAssetAtPath(path, typeof(T)) as T;
                _cache[path] = asset;
                return asset;
            }
        }
    }
}

#endif