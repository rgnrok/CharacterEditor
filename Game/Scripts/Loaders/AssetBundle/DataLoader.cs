using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CharacterEditor.JSONMap;

namespace CharacterEditor
{
    namespace AssetBundleLoader
    {
        public class DataLoader<T> : IDataLoader<T> where T : UnityEngine.Object, IData
        {
            private readonly Dictionary<string, GuidPathMap> _guids;
            private readonly ICommonLoader<T> _commonLoader;

            public DataLoader(Dictionary<string, GuidPathMap> guids, ICommonLoader<T> commonLoader)
            {
                _guids = guids;
                _commonLoader = commonLoader;
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

            public void LoadData(IList<string> guids, Action<Dictionary<string, T>> callback)
            {
                var paths = new List<string>(guids.Count);

                foreach (var guid in guids)
                {
                    if (!_guids.TryGetValue(guid, out var pathMap)) continue;
                    paths.Add(pathMap.path);
                }

                _commonLoader.LoadByPath(paths, callback);
            }

            public async Task<T> LoadData(string guid)
            {
                if (!_guids.TryGetValue(guid, out var pathMap))
                    return null;

                return await _commonLoader.LoadByPath(pathMap.path);
            }

            public async Task<Dictionary<string, T>> LoadData(IList<string> guids)
            {
                var guidDataDir = new Dictionary<string, T>(guids.Count);
                foreach (var guid in guids)
                {
                    if (!_guids.TryGetValue(guid, out var pathMap)) continue;
                    guidDataDir[guid] = await _commonLoader.LoadByPath(pathMap.path);
                }

                return guidDataDir;
            }

            public void CleanUp()
            {
                _commonLoader.CleanUp();
            }
        }
    }
}