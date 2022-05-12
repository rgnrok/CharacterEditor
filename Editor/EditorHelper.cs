using UnityEditor;

namespace Editor
{
    public static class EditorHelper
    {
        public static string GetObjectPath<T>(this T obj) where T : UnityEngine.Object
        {
            if (obj == null) return null;

            var path = AssetDatabase.GetAssetPath(obj);
            if (!string.IsNullOrEmpty(path)) return path;

            var prefab = PrefabUtility.GetCorrespondingObjectFromSource<T>(obj);
            path = AssetDatabase.GetAssetPath(prefab);

            return path;
        }
    }
}