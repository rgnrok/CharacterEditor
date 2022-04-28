using CharacterEditor;
using CharacterEditor.CharacterInventory;
using UnityEditor;
using UnityEngine;

public class Tools
{
    [MenuItem("Tools/Clear Prefs")]
    public static void ClearPrefs()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
    }

    [MenuItem("Tools/UpdateData")]
    public static void UpdateStaticDataPaths()
    {
        var oldPath = "Assets/Character_Editor/";
        var newPath = "Assets/Packages/Character_Editor/";

        var paths = AssetDatabase.FindAssets("t:ItemData", new string[] { "Assets/Game/Data" });
        foreach (var path in paths)
        {
            var itemData = AssetDatabase.LoadAssetAtPath<ItemData>(AssetDatabase.GUIDToAssetPath(path));

            itemData.prefabPath = itemData.prefabPath.Replace(oldPath, newPath);
            itemData.iconPath = itemData.prefabPath.Replace(oldPath, newPath);

            if (itemData is EquipItemData equipItemData)
            {
                foreach (var configItem in equipItemData.configsItems)
                {
                    configItem.configPath = configItem.configPath.Replace(oldPath, newPath);
                    foreach (var textureData in configItem.textures)
                    {
                        textureData.texturePath = textureData.texturePath.Replace(oldPath, newPath);
                    }
                    foreach (var model in configItem.models)
                    {
                        model.prefabPath = model.prefabPath.Replace(oldPath, newPath);
                        model.additionalPrefabPath = model.additionalPrefabPath.Replace(oldPath, newPath);
                        model.texturePath = model.texturePath.Replace(oldPath, newPath);
                        model.additionalTexturePath = model.additionalTexturePath.Replace(oldPath, newPath);
                    }
                }
            }

            EditorUtility.CopySerialized(itemData, itemData);
            AssetDatabase.SaveAssets();
        }

        var characterPaths = AssetDatabase.FindAssets("t:PlayerCharacterConfig", new string[] { "Assets/Game/Data" });
        foreach (var path in characterPaths)
        {
            var playerData = AssetDatabase.LoadAssetAtPath<PlayerCharacterConfig>(AssetDatabase.GUIDToAssetPath(path));

            playerData.faceMeshTexturePath = playerData.faceMeshTexturePath.Replace(oldPath, newPath);
            playerData.texturePath = playerData.texturePath.Replace(oldPath, newPath);
            playerData.portraitIconPath = playerData.portraitIconPath.Replace(oldPath, newPath);

            foreach (var faceMesh in playerData.faceMeshs)
            {
                faceMesh.meshPath = faceMesh.meshPath.Replace(oldPath, newPath);
            }


            EditorUtility.CopySerialized(playerData, playerData);
            AssetDatabase.SaveAssets();
        }
    }
}