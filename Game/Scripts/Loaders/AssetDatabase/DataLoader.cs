#if UNITY_EDITOR
using System;
using UnityEditor;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CharacterEditor
{
    namespace AssetDatabaseLoader
    {
        public abstract class DataLoader<T> : CommonLoader<T>, IDataLoader<T> where T: UnityEngine.Object, IData
        {
            protected abstract string ConfigsPath { get; }

            private Dictionary<string, T> _guidCache;


            private void PrepareGuidCache()
            {
                if (_guidCache != null && _guidCache.Count != 0) return;

                var stringType = typeof(T).Name;
                var paths = AssetDatabase.FindAssets("t:" + stringType, new string[] { ConfigsPath });

                _guidCache = new Dictionary<string, T>(paths.Length);
                foreach (var path in paths)
                {
                    var data = AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(path));
                    _guidCache[data.Guid] = data;
                }
            }

            public void LoadData(Action<Dictionary<string, T>> callback)
            {
                PrepareGuidCache();
                callback.Invoke(_guidCache);
            }

            public async Task<T> LoadData(string guid)
            {
                PrepareGuidCache();
                _guidCache.TryGetValue(guid, out var data);
                return data;
            }

            public void LoadData(string guid, Action<T> callback)
            {
                PrepareGuidCache();
                _guidCache.TryGetValue(guid, out var data);
                callback.Invoke(data);
            }

            public void LoadData(List<string> guids, Action<Dictionary<string, T>> callback)
            {
                callback.Invoke(InnerLoadData(guids));
            }

            public async Task<Dictionary<string, T>> LoadData(List<string> guids)
            {
                return InnerLoadData(guids);
            }

            private Dictionary<string, T> InnerLoadData(List<string> guids)
            {
                PrepareGuidCache();
                var dataItems = new Dictionary<string, T>(guids.Count);
                foreach (var guid in guids)
                {
                    if (_guidCache.TryGetValue(guid, out var data))
                        dataItems[data.Guid] = data;
                }

                return dataItems;
            }
        }
    }
}
#endif