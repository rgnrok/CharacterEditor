using System;
using CharacterEditor;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    public class SpriteProcessor : AssetPostprocessor
    {
        private static void SetTextureSettings(TextureImporter textureImporter, bool unCompressed)
        {
            textureImporter.textureType = TextureImporterType.Default;
            textureImporter.alphaIsTransparency = true;
            textureImporter.isReadable = true;
            textureImporter.npotScale = TextureImporterNPOTScale.None;
            textureImporter.mipmapEnabled = false;

            if (unCompressed)
                textureImporter.textureCompression = TextureImporterCompression.Uncompressed;
        }

        void OnPostprocessTexture(Texture2D texture)
        {
            if (assetPath.IndexOf("Game/Data/PlayableNpc", StringComparison.Ordinal) == -1)
                return;

            ImportAsset(assetPath, true);
        }

        [MenuItem("Tools/Character Editor/Update Textures")]
        public static void UpdateTextures()
        {
            var textures = AssetDatabase.FindAssets("t:texture2D", new string[]
                {
                    $"{AssetsConstants.CharacterEditorRootPath}/Prefabs",
                    $"{AssetsConstants.CharacterEditorRootPath}/Textures/Character",
                }
            );

            for (var i = 0; i < textures.Length; i++)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(textures[i]);
                ImportAsset(assetPath);
            }
        }

        private static void ImportAsset(string assetPath, bool unCompressed = false)
        {
            var textureImporter = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (textureImporter == null) return;

            SetTextureSettings(textureImporter, unCompressed);

            AssetDatabase.ImportAsset(assetPath);
            AssetDatabase.Refresh();
        }
    }
}