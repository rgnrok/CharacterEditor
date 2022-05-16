#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

public static partial class Helper
{
    public static bool TryCreateFolder(string dir)
    {
        if (AssetDatabase.IsValidFolder(dir)) return true;

        var folderParts = dir.Split('/');
        var rootFolder = folderParts[0];
        for (var i = 1; i < folderParts.Length; i++)
        {
            var folder = $"{rootFolder}/{folderParts[i]}";
            if (!AssetDatabase.IsValidFolder(folder))
            {
                var guid = AssetDatabase.CreateFolder(rootFolder, folderParts[i]);
                if (string.IsNullOrEmpty(guid)) return false;
            }
            rootFolder = folder;
        }
        return true;
    }



        public static Texture2D DeCompress(this Texture2D source)
        {
            var renderTex = RenderTexture.GetTemporary(
                source.width,
                source.height,
                0,
                RenderTextureFormat.Default,
                RenderTextureReadWrite.Linear);

            Graphics.Blit(source, renderTex);

            RenderTexture.active = renderTex;
            var newTexture = new Texture2D(source.width, source.height);
            newTexture.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
            newTexture.Apply();
   
            RenderTexture.ReleaseTemporary(renderTex);
            return newTexture;
    }
}
#endif

