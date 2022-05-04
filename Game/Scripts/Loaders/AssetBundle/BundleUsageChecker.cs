using System.Collections.Generic;

namespace CharacterEditor
{
    public static class BundleUsageChecker
    {
        private static readonly Dictionary<string, int> _bundleUsageCounters = new Dictionary<string, int>();

        public static bool CheckUnload(string assetBundleName)
        {
            if (!_bundleUsageCounters.ContainsKey(assetBundleName)) return true;

            var bundleUsageCounter = _bundleUsageCounters[assetBundleName];
            bundleUsageCounter--;
            if (bundleUsageCounter > 0) return false;

            return true;
        }

        public static void UpdateUsageCounter(string assetBundleName)
        {
            if (!_bundleUsageCounters.ContainsKey(assetBundleName))
                _bundleUsageCounters[assetBundleName] = 0;

            _bundleUsageCounters[assetBundleName]++;
        }
    }
}