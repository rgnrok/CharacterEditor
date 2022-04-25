using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace CharacterEditor.Services
{
    public class AssetsProvider : IAssets
    {
        private Dictionary<string, AsyncOperationHandle> _completedHandles = new Dictionary<string, AsyncOperationHandle>();
        private AsyncOperationHandle<IResourceLocator> _initializeOperation;

        public void InitializeAsync()
        {
            _initializeOperation = Addressables.InitializeAsync();
        }

        public async Task<T> Load<T>(AssetReference assetReference) where T : class
        {
            if (_completedHandles.TryGetValue(assetReference.AssetGUID, out var completeHandle))
            {
                if (completeHandle.IsDone) return completeHandle.Result as T;
                return await completeHandle.Task as T;
            }

            AsyncOperationHandle<T> asyncOperationHandle = Addressables.LoadAssetAsync<T>(assetReference);
            return await LoadWithCache<T>(asyncOperationHandle, assetReference.AssetGUID);
        }

        public async Task<T> Load<T>(string assetPath) where T : class
        {
            if (_completedHandles.TryGetValue(assetPath, out var completeHandle))
            {
                if (completeHandle.IsDone) return completeHandle.Result as T;
                return await completeHandle.Task as T;
            }

            AsyncOperationHandle<T> asyncOperationHandle = Addressables.LoadAssetAsync<T>(assetPath);
            return await LoadWithCache<T>(asyncOperationHandle, assetPath);
        }


        public GameObject Instantiate(string path)
        {
            var prefab = Resources.Load<GameObject>(path);
            return Object.Instantiate(prefab);
        }

        public GameObject Instantiate(GameObject prefab)
        {
            return Object.Instantiate(prefab);
        }

        public Task<GameObject> InstantiateAsync(string address)
        {
            return Addressables.InstantiateAsync(address).Task;
        }

        public Task<GameObject> InstantiateAsync(string address, Vector3 at)
        {
            return Addressables.InstantiateAsync(address, at, Quaternion.identity).Task;
        }

        public void CleanUp()
        {
            foreach (var operationHandler in _completedHandles.Values)
                Addressables.Release(operationHandler);

            _completedHandles.Clear();
        }

        private async Task<T> LoadWithCache<T>(AsyncOperationHandle<T> asyncOperationHandle, string cacheKey) where T : class
        {
            _completedHandles[cacheKey] = asyncOperationHandle;
            return await asyncOperationHandle.Task;
        }
    }
}