using System;
using CharacterEditor;
using CharacterEditor.CharacterInventory;
using UnityEditor;
using UnityEngine;

namespace Editor
{
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
                var playerData =
                    AssetDatabase.LoadAssetAtPath<PlayableNpcConfig>(AssetDatabase.GUIDToAssetPath(path));

                playerData.faceMeshTexturePath.path = playerData.faceMeshTexturePath.path.Replace(oldPath, newPath);
                playerData.texturePath.path = playerData.texturePath.path.Replace(oldPath, newPath);
                playerData.portraitIconPath = playerData.portraitIconPath.Replace(oldPath, newPath);

                foreach (var faceMesh in playerData.faceMeshs)
                {
                    faceMesh.meshPath.path = faceMesh.meshPath.path.Replace(oldPath, newPath);
                }

                EditorUtility.CopySerialized(playerData, playerData);
                AssetDatabase.SaveAssets();
            }
        }

        [MenuItem("Tools/UpdateLodGroups")]
        public static void UpdateLodGroups()
        {
            try
            {
                UpdateLodGroupsInner();
            }
            catch (Exception e)
            {
                Debug.LogError($"Error: {e.Message}\n {e.StackTrace}");
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        private static void UpdateLodGroupsInner()
        {
            var prefabPaths = AssetDatabase.FindAssets("t:GameObject",
                new[] { "Assets/Packages/Character_Editor/Prefabs", "Assets/Packages/Character_Editor/NewCharacter" });

            float prefabsCount = prefabPaths.Length;
            for (var i = 0; i < prefabsCount; i++)
            {
                var path = prefabPaths[i];
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(path));
                var loadGroups = prefab.GetComponentsInChildren<LODGroup>();

                foreach (var loadGroup in loadGroups)
                {
                    var lods = loadGroup.GetLODs();
                    if (lods.Length < 3) continue;

                    lods[0].screenRelativeTransitionHeight = 0.65f;
                    lods[1].screenRelativeTransitionHeight = 0.15f;

                    loadGroup.SetLODs(lods);
                    loadGroup.RecalculateBounds();

                    lods = loadGroup.GetLODs();
                    for (var j = 0; j < lods.Length; j++)
                        Debug.Log($"{prefab.name} has {j} lod with {lods[j].screenRelativeTransitionHeight * 100}%");
                }



                EditorUtility.DisplayCancelableProgressBar($"Update LODs: {prefab.name}", string.Empty, i / prefabsCount);
            }
        }
    }
}