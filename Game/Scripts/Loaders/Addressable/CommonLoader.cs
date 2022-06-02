using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace CharacterEditor
{
    namespace AddressableLoader
    {
        public class CommonLoader<T> : ICommonLoader<T> where T : UnityEngine.Object
        {
            private readonly Dictionary<string, AsyncOperationHandle> _completedHandles = new Dictionary<string, AsyncOperationHandle>();

            public async void LoadByPath(string path, Action<string, T> callback)
            {
                var result = await LoadByPath(path);
                callback?.Invoke(path, result);
            }

            public async Task<T> LoadByPath(string path)
            {
                if (_completedHandles.TryGetValue(path, out var completeHandle))
                {
                    if (completeHandle.IsDone) return completeHandle.Result as T;
                    return await completeHandle.Task as T;
                }

                AsyncOperationHandle<T> asyncOperationHandle = Addressables.LoadAssetAsync<T>(path);
                return await LoadWithCache(asyncOperationHandle, path);
            }


            public async Task<Dictionary<string, T>> LoadByPath(IList<string> paths)
            {
                var result = new Dictionary<string, T>(paths.Count);
                foreach (var path in paths)
                    result[path] = await LoadByPath(path);

                return result;
            }

            public async void LoadByPath(IList<string> paths, Action<Dictionary<string, T>> callback)
            {
                var result = await LoadByPath(paths);
                callback?.Invoke(result);
            }

            public void Unload(string path)
            {
                if (!_completedHandles.TryGetValue(path, out var handler)) return;

                Addressables.Release(handler);
                _completedHandles.Remove(path);
            }

            public void CleanUp()
            {
                foreach (var operationHandler in _completedHandles.Values)
                    Addressables.Release(operationHandler);

                _completedHandles.Clear();
            }

            private async Task<T> LoadWithCache(AsyncOperationHandle<T> asyncOperationHandle, string cacheKey) 
            {
                _completedHandles[cacheKey] = asyncOperationHandle;
                return await asyncOperationHandle.Task;
            }
        }
    }
}