using System.Collections.Generic;
using UnityEngine;

namespace CharacterEditor
{
    public static class BundleUsageChecker
    {
        private static readonly Dictionary<string, int> _bundleUsageCounters = new Dictionary<string, int>();

        public static bool CheckUnload(string assetBundleName)
        {
            _bundleUsageCounters.TryGetValue(assetBundleName, out var count);
            Logger.LogWarning($"Unload assetBundleName: {assetBundleName}. UsageCounters: {count}");

            if (_bundleUsageCounters.ContainsKey(assetBundleName))
            {
                _bundleUsageCounters[assetBundleName]--;
                if (_bundleUsageCounters[assetBundleName] > 0) return false;
            }

            return true;
        }

        public static void UpdateUsageCounter(string assetBundleName)
        {
            if (!_bundleUsageCounters.ContainsKey(assetBundleName))
                _bundleUsageCounters[assetBundleName] = 0;

            _bundleUsageCounters[assetBundleName]++;

            Logger.LogWarning($"UpdateUsageCounter  {assetBundleName}. UsageCounters: { _bundleUsageCounters[assetBundleName] }");
        }
    }
}