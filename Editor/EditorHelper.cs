using CharacterEditor;
using System.IO;
using UnityEditor;
using UnityEngine;

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

        public static string SaveTexture(string directory, Texture2D texture)
        {
            Helper.TryCreateFolder(directory);

            var path = $"{directory}/{texture.name}.png";
            File.WriteAllBytes(path, texture.DeCompress().EncodeToPNG());

            return path;
        }

        public static string SaveAsset(string directory, string name, Object asset)
        {
            Helper.TryCreateFolder(directory);

            var path = $"{directory}/{name}";
            var currentPath = AssetDatabase.GetAssetPath(asset);
            if (!string.IsNullOrEmpty(currentPath))
                AssetDatabase.CopyAsset(currentPath, path);
            else
                AssetDatabase.CreateAsset(asset, path);

            AssetDatabase.Refresh();

            return path;
        }
    }
}