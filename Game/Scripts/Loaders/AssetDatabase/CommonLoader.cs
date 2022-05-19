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

            public Task<T> LoadByPath(string path)
            {
                return Task.FromResult(InnerLoadByPath(path));
            }

            public Task<Dictionary<string, T>> LoadByPath(IList<string> paths)
            {
                return Task.FromResult(InnerLoadByPath(paths));
            }

            public void LoadByPath(string path, Action<string, T> callback)
            {
                callback?.Invoke(path, InnerLoadByPath(path));
            }

            public void LoadByPath(IList<string> paths, Action<Dictionary<string, T>> callback)
            {
                var dataItems = InnerLoadByPath(paths);
                callback.Invoke(dataItems);
            }

            public void Unload(string path)
            {
                Resources.UnloadUnusedAssets();
            }

            public void CleanUp()
            {
                Resources.UnloadUnusedAssets();
            }

            private T InnerLoadByPath(string path)
            {
                if (_cache.TryGetValue(path, out var asset))
                    return asset;

                asset = AssetDatabase.LoadAssetAtPath<T>(path);
                _cache[path] = asset;
                return asset;
            }

            private Dictionary<string, T> InnerLoadByPath(IList<string> paths)
            {
                var dataItems = new Dictionary<string, T>();
                foreach (var path in paths)
                {
                    var asset = InnerLoadByPath(path);
                    if (asset == null) continue;

                    dataItems[path] = asset;
                }

                return dataItems;
            }
        }
    }
}

#endif