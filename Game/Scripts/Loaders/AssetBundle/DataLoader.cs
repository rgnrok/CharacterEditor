using System;
using System.Collections.Generic;
using CharacterEditor.JSONMap;
using Game;

namespace CharacterEditor
{
    namespace AssetBundleLoader
    {
        public class DataLoader<T> : IDataLoader<T> where T : UnityEngine.Object, IData
        {
            private readonly Dictionary<string, GuidPathMap> _guids;
            private readonly ICommonLoader<T> _commonLoader;

            public DataLoader(Dictionary<string, GuidPathMap> guids, ICoroutineRunner coroutineRunner)
            {
                _guids = guids;
                _commonLoader = new CommonLoader<T>(coroutineRunner);
            }

            public void LoadData(List<string> guids, Action<Dictionary<string, T>> callback)
            {
                var paths = new List<string>(guids.Capacity);

                foreach (var guid in guids)
                {
                    if (!_guids.TryGetValue(guid, out var pathMap)) continue;
                    paths.Add(pathMap.path);
                }

                _commonLoader.LoadByPath(paths, callback);
            }

            public void LoadData(string guid, Action<T> callback)
            {
                if (!_guids.TryGetValue(guid, out var pathMap))
                {
                    callback.Invoke(null);
                    return;
                }

                _commonLoader.LoadByPath(pathMap.path, (path, entity) => callback(entity));
            }
        }
    }
}