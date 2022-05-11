using CharacterEditor;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    public class SpriteProcessor : AssetPostprocessor
    {
        private static void SetTextureSettings(TextureImporter textureImporter)
        {
            textureImporter.textureType = TextureImporterType.Default;
            textureImporter.alphaIsTransparency = true;
            textureImporter.isReadable = true;
            textureImporter.npotScale = TextureImporterNPOTScale.None;
            textureImporter.textureCompression = TextureImporterCompression.Uncompressed;
            textureImporter.mipmapEnabled = false;
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

            for (int i = 0; i < textures.Length; i++)
            {
                var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(textures[i]));
                var assetPath = AssetDatabase.GetAssetPath(texture);
                var textureImporter = AssetImporter.GetAtPath(AssetDatabase.GUIDToAssetPath(textures[i])) as TextureImporter;

                if (textureImporter != null)
                {
                    SetTextureSettings(textureImporter);

                    AssetDatabase.ImportAsset(assetPath);
                    AssetDatabase.Refresh();
                }
            }
        }
    }
}