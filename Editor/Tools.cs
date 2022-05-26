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

        [MenuItem("Tools/Delete Saves")]
        public static void DeleteSaves()
        {
            FileSaveLoadStorage.DeleteSaves();
            ClearPrefs();
        }

        [MenuItem("Tools/UpdateData")]
        public static void UpdateStaticDataPaths()
        {
            var paths = AssetDatabase.FindAssets("t:EquipItemData", new string[] { "Assets/Game/Data" });
            foreach (var path in paths)
            {
                var equipItemData = AssetDatabase.LoadAssetAtPath<EquipItemData>(AssetDatabase.GUIDToAssetPath(path));

              
                    // foreach (var configItem in equipItemData.configsItems)
                    // {
                    //     foreach (var textureData in configItem.textures)
                    //     {
                    //         if (!string.IsNullOrEmpty(textureData.texturePath))
                    //             textureData.texture.path = textureData.texturePath;
                    //     }
                    //
                    //     foreach (var model in configItem.models)
                    //     {
                    //         if (!string.IsNullOrEmpty(model.texturePath))
                    //             model.texture.path = model.texturePath;
                    //         if (!string.IsNullOrEmpty(model.additionalTexturePath))
                    //             model.additionalTexture.path = model.additionalTexturePath;
                    //
                    //         if (!string.IsNullOrEmpty(model.prefabPath))
                    //             model.prefab.path = model.prefabPath;
                    //         if (!string.IsNullOrEmpty(model.additionalPrefabPath))
                    //             model.additionalPrefab.path = model.additionalPrefabPath;
                    //     }
                    // }
                

                EditorUtility.CopySerialized(equipItemData, equipItemData);
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

                    lods[0].screenRelativeTransitionHeight = 0.55f;
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