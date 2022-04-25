using System;
using System.Collections;
using System.Collections.Generic;
using AssetBundles;
using Game;
using UnityEngine;

namespace CharacterEditor
{
    namespace AssetBundleLoader
    {
        public class CommonLoader<T> : ICommonLoader<T> where T : UnityEngine.Object
        {
            private readonly Dictionary<string, T> _cache = new Dictionary<string, T>();
            private readonly Dictionary<string, int> _bundleUsageCounters = new Dictionary<string, int>();
            private readonly ICoroutineRunner _coroutineRunner;

            public CommonLoader(ICoroutineRunner coroutineRunner)
            {
                _coroutineRunner = coroutineRunner;
            }

            public void Unload(string path)
            {
                var (assetBundleName, _) = ParseAssetName(path);

                if (_bundleUsageCounters.ContainsKey(assetBundleName))
                {
                    _bundleUsageCounters[assetBundleName]--;
                    if (_bundleUsageCounters[assetBundleName] > 0) return;
                }

                AssetBundleManager.UnloadAssetBundle(assetBundleName, true);
                _cache.Remove(path);
                Resources.UnloadUnusedAssets();
            }

            public void LoadByPath(string path, Action<string, T> callback)
            {
                _coroutineRunner.StartCoroutine(LoadByPathCoroutine(path, (asset) => callback?.Invoke(path, asset)));
            }

            public void LoadByPath(List<string> paths, Action<Dictionary<string, T>> callback)
            {
                _coroutineRunner.StartCoroutine(LoadByPathCoroutine(paths, callback));
            }

            private IEnumerator LoadByPathCoroutine(List<string> paths, Action<Dictionary<string, T>> callback)
            {
                var dataItems = new Dictionary<string, T>();
                foreach (var path in paths)
                {
                    yield return LoadByPathCoroutine(path, datItem => { dataItems[path] = datItem; });
                }
                callback.Invoke(dataItems);
            }

            protected IEnumerator LoadByPathCoroutine(string path, Action<T> callback)
            {
                var (assetBundleName, assetName) = ParseAssetName(path);

                if (_cache.TryGetValue(path, out var asset))
                {
                    UpdateUsageCounter(assetBundleName);
                    callback?.Invoke(asset);
                    yield break;
                }

                var request = AssetBundleManager.LoadAssetAsync(assetBundleName, assetName, typeof(T));
                if (request == null)
                {
                    Debug.LogError("Failed AssetBundleLoadAssetOperation on " + assetName + " from the AssetBundle " + assetBundleName + ".");
                    callback?.Invoke(null);
                    yield break;
                }
                yield return request;

                asset = request.GetAsset<T>();

                _cache[path] = asset;
                UpdateUsageCounter(assetBundleName);

                callback?.Invoke(asset);
            }

            private void UpdateUsageCounter(string assetBundleName)
            {
                if (!_bundleUsageCounters.ContainsKey(assetBundleName))
                    _bundleUsageCounters[assetBundleName] = 0;

                _bundleUsageCounters[assetBundleName]++;
            }

            private static (string, string) ParseAssetName(string path)
            {
                var pathParts = path.Split('/');

                var bundleName = pathParts[0].ToLower();
                var assetName = pathParts[pathParts.Length - 1];
                return (bundleName, assetName);
            }
        }
    }
}