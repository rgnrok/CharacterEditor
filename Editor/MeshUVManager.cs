using System;
using System.Collections.Generic;
using System.IO;
using CharacterEditor;
using CharacterEditor.AssetDatabaseLoader;
using CharacterEditor.CharacterInventory;
using CharacterEditor.Mesh;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public class MeshUVManager
{
    [MenuItem("Tools/Character Editor/Update Mesh UV")]
    public static void UpdateMeshUV()
    {
        var configLoader = new ConfigLoader();
        var configs = configLoader.LoadConfigs().Result;
        UpdateUVs(configs);
    }

    private static void UpdateUVs(CharacterConfig[] configs)
    {
        foreach (var config in configs)
        {
            var armorMeshFolderPath = $"{AssetsConstants.CharacterEditorRootPath}/Meshes/{config.folderName}/Armor/";
            var weaponMeshFolderPath = $"{AssetsConstants.CharacterEditorRootPath}/Meshes/{config.folderName}/Weapon/";

            var skinPaths = new List<string>()
            {
                armorMeshFolderPath + "Armor_Hair",
                armorMeshFolderPath + "Armor_Jaw",
                armorMeshFolderPath + "Armor_Feature",
            };

            var meshPaths = new SortedDictionary<int, string>()
            {
                {MeshFactory.GetMeshMergeOrder(MeshType.ArmRight), armorMeshFolderPath + "Armor_Arm/ArmRight"},
                {MeshFactory.GetMeshMergeOrder(MeshType.ArmLeft), armorMeshFolderPath + "Armor_Arm/ArmLeft"},
                {MeshFactory.GetMeshMergeOrder(MeshType.Belt), armorMeshFolderPath + "Armor_Belt"},
                {MeshFactory.GetMeshMergeOrder(MeshType.BeltAdd), armorMeshFolderPath + "Armor_BeltAdd"},
                {MeshFactory.GetMeshMergeOrder(MeshType.Helm), armorMeshFolderPath + "Armor_Helm"},
                {MeshFactory.GetMeshMergeOrder(MeshType.LegRight), armorMeshFolderPath + "Armor_Leg/LegRight"},
                {MeshFactory.GetMeshMergeOrder(MeshType.LegLeft), armorMeshFolderPath + "Armor_Leg/LegLeft"},
                {MeshFactory.GetMeshMergeOrder(MeshType.ShoulderRight), armorMeshFolderPath + "Armor_Shoulder/ShoulderRight"},
                {MeshFactory.GetMeshMergeOrder(MeshType.ShoulderLeft), armorMeshFolderPath + "Armor_Shoulder/ShoulderLeft"},
                {MeshFactory.GetMeshMergeOrder(MeshType.Torso), armorMeshFolderPath + "Armor_Torso"},
                {MeshFactory.GetMeshMergeOrder(MeshType.TorsoAdd), armorMeshFolderPath + "Armor_TorsoAdd"},
                {MeshFactory.GetMeshMergeOrder(MeshType.HandRight), weaponMeshFolderPath + "HandRight"},
                {MeshFactory.GetMeshMergeOrder(MeshType.HandLeft), weaponMeshFolderPath + "HandLeft"}
            };

            UpdatePathsUVs(config.folderName, meshPaths.Values, 4); // 2048/512 px
            UpdatePathsUVs(config.folderName, skinPaths, 2); // 1024/512 px
        }
    }


    private static void UpdatePathsUVs(string folderName, IEnumerable<string> meshPaths, int atlasSize)
    {
        float uvsStep = 1f / atlasSize;
        int itemNum = 0;
        foreach(var meshPath in meshPaths)
        {
            if (!AssetDatabase.IsValidFolder(meshPath)) continue;

            var meshGUIDs = AssetDatabase.FindAssets("t:GameObject", new string[] { meshPath });
            for (int meshNum = 0; meshNum < meshGUIDs.Length; meshNum++)
            {
                var tempMesh = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(meshGUIDs[meshNum]));

                //Update LOD parts for each armor item
                var armorsParts = tempMesh.GetComponentsInChildren<MeshFilter>();
                for (var armLOD = 0; armLOD < armorsParts.Length; armLOD++)
                {
                    if (armorsParts[armLOD] != null)
                    {
                        var mTempMesh = (Mesh) GameObject.Instantiate(armorsParts[armLOD].sharedMesh);

                        //Update UVS for new atlas
                        Vector2[] uvs = mTempMesh.uv;
                        for (int i = 0; i < uvs.Length; i++)
                        {
                            uvs[i] = new Vector2(uvs[i].x / atlasSize + uvsStep * (itemNum % atlasSize),
                                uvs[i].y / atlasSize + uvsStep * (atlasSize - 1 - (itemNum / atlasSize)));
                        }

                        mTempMesh.uv = uvs;
                        //assigne the selected LOD Mesh with new UV's to the new mesh to be exported
                        if (!Directory.Exists(meshPath + "/Meshes/"))
                            Directory.CreateDirectory(meshPath + "/Meshes/");

                        CreateOrReplaceAsset<Mesh>(mTempMesh, meshPath + "/Meshes/" + armorsParts[armLOD].sharedMesh.name + "_New.asset");
                        AssetDatabase.SaveAssets();
                    }
                }
            }

            itemNum++;
        }

        var prefabsPath = $"{ AssetsConstants.CharacterEditorRootPath}/Prefabs/" + folderName;
        var prefabGUIDs = AssetDatabase.FindAssets("t:Prefab", new string[] {prefabsPath});
        for (int prefNum = 0; prefNum < prefabGUIDs.Length; prefNum++)
        {
            var pPath = AssetDatabase.GUIDToAssetPath(prefabGUIDs[prefNum]);
            if (pPath.Contains("/Model/"))
            {
                var originalPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(pPath);
                var originalPrefabInstance = GameObject.Instantiate(originalPrefab);
                originalPrefabInstance.name = originalPrefab.name;
                Debug.Log("GET MODEL " + originalPrefabInstance.name);
                //Remove materials
                foreach (var meshRender in originalPrefabInstance.GetComponentsInChildren<MeshRenderer>())
                {
                    meshRender.materials = new Material[0];
                    meshRender.material = null;
                }

                foreach (var filter in originalPrefabInstance.GetComponentsInChildren<MeshFilter>())
                {
                    var lodMeshPath = AssetDatabase.GetAssetPath(filter.sharedMesh);
                    var index = lodMeshPath.LastIndexOf("/");
                    if (index != -1)
                    {
                        lodMeshPath = lodMeshPath.Substring(0, index) + "/Meshes/" + filter.sharedMesh.name + "_New.asset";
                        var changedMesh = AssetDatabase.LoadAssetAtPath<Mesh>(lodMeshPath);
                        filter.mesh = changedMesh;
                    }
                }

                var newDirPath = pPath.Substring(0, pPath.IndexOf("Model/")) + "StaticModel/";
                var fullDirPath = Application.dataPath.Substring(0, Application.dataPath.Length - 6) + newDirPath;

                if (Directory.Exists(fullDirPath))
                    Directory.Delete(fullDirPath, true);

                Directory.CreateDirectory(fullDirPath);

                var prefabPath = newDirPath + originalPrefabInstance.name + ".prefab";
                Debug.Log(prefabPath);

#if UNITY_2018_3_OR_NEWER
                PrefabUtility.SaveAsPrefabAsset(originalPrefabInstance, prefabPath);
#else
                Object prefab = PrefabUtility.CreateEmptyPrefab(prefabPath);
                PrefabUtility.ReplacePrefab(originalPrefabInstance, prefab, ReplacePrefabOptions.ConnectToPrefab);
#endif

                AssetDatabase.SaveAssets();

                Debug.Log("DESTROY " + originalPrefabInstance.name);

                GameObject.DestroyImmediate(originalPrefabInstance);
            }
        }
    }
    
    private static T CreateOrReplaceAsset<T>(T asset, string path) where T : Object {
        T existingAsset = AssetDatabase.LoadAssetAtPath<T>(path);

        if (existingAsset == null) {
            AssetDatabase.CreateAsset(asset, path);
            existingAsset = asset;
        }
        else {
            EditorUtility.CopySerialized(asset, existingAsset);
        }

        return existingAsset;
    }
}