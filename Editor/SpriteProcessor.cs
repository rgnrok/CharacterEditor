using CharacterEditor;
using UnityEditor;
using UnityEngine;

public class SpriteProcessor : AssetPostprocessor
{
    /* 
    //Simplifies the addition of textures to the finished editor. When you import a package, it takes too long. Uncomment if necessary
    void OnPostprocessTexture(Texture2D texture)
    {
        string lowerCaseAssetPath = assetPath.ToLower();
        bool isInSpritesDirectory = lowerCaseAssetPath.IndexOf("/sprites/") != -1;

        bool isInTextureDirectory = lowerCaseAssetPath.IndexOf("/character_editor/textures/character/") != -1 ||
                                    lowerCaseAssetPath.IndexOf("/character_editor/prefabs/") != -1;

        if (isInSpritesDirectory)
        {
            TextureImporter textureImporter = (TextureImporter) assetImporter;
            textureImporter.textureType = TextureImporterType.Sprite;
            textureImporter.spriteImportMode = SpriteImportMode.Single;
            textureImporter.mipmapEnabled = false;
            return;
        }
        if (isInTextureDirectory)
        {
            TextureImporter textureImporter = (TextureImporter)assetImporter;
            SetTextureSettings(textureImporter);

            AssetDatabase.Refresh();
        }
    }
    */

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
            string assetPath = AssetDatabase.GetAssetPath(texture);
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