#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CharacterEditor
{
    namespace AssetDatabaseLoader
    {
        public class CommonLoader<T> : ICommonLoader<T> where T : UnityEngine.Object
        {
            private readonly Dictionary<string, T> _cache = new Dictionary<string, T>();


            public void LoadByPath(string path, Action<string, T> callback)
            {
                if (_cache.TryGetValue(path, out var asset))
                {
                    callback?.Invoke(path, asset);
                    return;
                }

                asset = AssetDatabase.LoadAssetAtPath(path, typeof(T)) as T;
                _cache[path] = asset;
                callback?.Invoke(path, asset);
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
        }
    }
}

#endif