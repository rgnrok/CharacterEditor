﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using AssetBundles;
using Game;

namespace CharacterEditor
{
    namespace AssetBundleLoader
    {
        public class CommonLoader<T> : ICommonLoader<T> where T : UnityEngine.Object
        {
            private readonly Dictionary<string, T> _cache = new Dictionary<string, T>();
            private readonly ICoroutineRunner _coroutineRunner;

            public CommonLoader(ICoroutineRunner coroutineRunner)
            {
                _coroutineRunner = coroutineRunner;
            }

            public void LoadByPath(string path, Action<string, T> callback)
            {
                _coroutineRunner.StartCoroutine(LoadByPathCoroutine(path, (asset) => callback?.Invoke(path, asset)));
            }

            public async Task<T> LoadByPath(string path)
            {
                T result = null;
                var isComplete = false;
                var c = _coroutineRunner.StartCoroutine(LoadByPathCoroutine(path, (asset) =>
                {
                    result = asset;
                    isComplete = true;
                }));
                while (!isComplete) await Task.Yield();
                return result;
            }

            public async Task<Dictionary<string, T>> LoadByPath(IList<string> paths)
            {
                var totalCount = paths.Count;
                var result = new Dictionary<string, T>(totalCount);
                var completeCount = 0;
                foreach (var path in paths)
                {
                    _coroutineRunner.StartCoroutine(LoadByPathCoroutine(path, (asset) =>
                    {
                        result[path] = asset;
                        completeCount++;
                    }));
                }
                while (completeCount != totalCount) await Task.Yield();
                return result;
            }

            public void LoadByPath(IList<string> paths, Action<Dictionary<string, T>> callback)
            {
                _coroutineRunner.StartCoroutine(LoadByPathCoroutine(paths, callback));
            }

            public void Unload(string path)
            {
                UnloadFromBundle(path);
                _cache.Remove(path);
            }

            public void CleanUp()
            {
                foreach (var path in _cache.Keys)
                    UnloadFromBundle(path);

                _cache.Clear();
            }

            private static void UnloadFromBundle(string path)
            {
                var (assetBundleName, _) = ParseAssetName(path);
                if (!BundleUsageChecker.CheckUnload(assetBundleName)) return;

                AssetBundleManager.UnloadAssetBundle(assetBundleName, true);
            }

            private IEnumerator LoadByPathCoroutine(IList<string> paths, Action<Dictionary<string, T>> callback)
            {
                var dataItems = new Dictionary<string, T>();
                foreach (var path in paths)
                    yield return LoadByPathCoroutine(path, datItem => { dataItems[path] = datItem; });

                callback.Invoke(dataItems);
            }

            private IEnumerator LoadByPathCoroutine(string path, Action<T> callback)
            {
                var (assetBundleName, assetName) = ParseAssetName(path);
                if (_cache.TryGetValue(path, out var asset))
                {
                    BundleUsageChecker.UpdateUsageCounter(assetBundleName);
                    callback?.Invoke(asset);
                    yield break;
                }

                var request = AssetBundleManager.LoadAssetAsync(assetBundleName, assetName, typeof(T));
                if (request == null)
                {
                    Logger.LogError("Failed AssetBundleLoadAssetOperation on " + assetName + " from the AssetBundle " + assetBundleName + ".");
                    callback?.Invoke(null);
                    yield break;
                }
                yield return request;

                asset = request.GetAsset<T>();

                _cache[path] = asset;
                BundleUsageChecker.UpdateUsageCounter(assetBundleName);

                callback?.Invoke(asset);
            }

            private static (string, string) ParseAssetName(string path)
            {
                var index = path.IndexOf('/');
                var bundleName = path.Substring(0, index).ToLower();
                var assetName = path.Substring(index+1);

                return (bundleName, assetName);
            }
        }
    }
}