#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using CharacterEditor.CharacterInventory;
using UnityEditor;

namespace CharacterEditor
{
    namespace AssetDatabaseLoader
    {
        /*
         * Parse and Load Meshes (Only editor)
         */
        public class MeshLoader : CommonLoader<GameObject>, IMeshLoader
        {
            private readonly TextureLoader _textureLoader;
            private MeshAtlasType meshAtlasType;

            public MeshLoader(TextureLoader textureLoader, MeshAtlasType atlasType)
            {
                _textureLoader = textureLoader;
                meshAtlasType = atlasType;
            }

            public MeshTexture CreateMeshTexture(string[][] textures) =>
                new MeshTexture(_textureLoader, textures);

            public Dictionary<string, string[][]> ParseMeshes(string race, MeshType meshType)
            {
                var folderPath = GetFolderPath(race, meshType);
                if (folderPath == null) return null;

                var dirPath = Path.Combine(Application.dataPath, folderPath.Substring(7));
                if (!Directory.Exists(dirPath)) return null;

                var folders = Directory.GetDirectories(dirPath);
                var meshAndTexturePaths = new Dictionary<string, string[][]>();

                var modelType = meshAtlasType == MeshAtlasType.Static ? "StaticModel" : "Model";
                for (var i = 0; i < folders.Length; i++)
                {
                    var path = folders[i].Substring(Application.dataPath.Length - 6);
                    var meshGUIDs = AssetDatabase.FindAssets("t:GameObject", new [] { path + "/" + modelType });
                    var meshPath = AssetDatabase.GUIDToAssetPath(meshGUIDs[0]);

                    meshAndTexturePaths[meshPath] = _textureLoader.ParseTextures(path + "/Textures");
                }

                return meshAndTexturePaths;
            }


            public void LoadItemMesh(string meshPath, string texturePath, Action<GameObject, ItemTexture> callback)
            {
                var meshObject = AssetDatabase.LoadAssetAtPath(meshPath, typeof(GameObject)) as GameObject;
                var texture = new ItemTexture(_textureLoader, texturePath);

                callback.Invoke(meshObject, texture);
            }
            
            private string GetFolderPath(string characterRace, MeshType meshType)
            {
                switch (meshType)
                {
                    case MeshType.ArmLeft:
                        return string.Format(AssetsConstants.MeshPathTemplate, characterRace, "Arm L");
                    case MeshType.ArmRight:
                        return string.Format(AssetsConstants.MeshPathTemplate, characterRace, "Arm R");
                    case MeshType.HandLeft:
                        return string.Format(AssetsConstants.MeshPathTemplate, characterRace, "Hand L");
                    case MeshType.HandRight:
                        return string.Format(AssetsConstants.MeshPathTemplate, characterRace, "Hand R");
                    case MeshType.LegLeft:
                        return string.Format(AssetsConstants.MeshPathTemplate, characterRace, "Leg L");
                    case MeshType.LegRight:
                        return string.Format(AssetsConstants.MeshPathTemplate, characterRace, "Leg R");
                    case MeshType.ShoulderLeft:
                        return string.Format(AssetsConstants.MeshPathTemplate, characterRace, "Shoulder L");
                    case MeshType.ShoulderRight:
                        return string.Format(AssetsConstants.MeshPathTemplate, characterRace, "Shoulder R");
                    case MeshType.Helm:
                        return string.Format(AssetsConstants.MeshPathTemplate, characterRace, "Head");
                    case MeshType.Beard:
                    case MeshType.Belt:
                    case MeshType.BeltAdd:
                    case MeshType.FaceFeature:
                    case MeshType.Hair:
                    case MeshType.Torso:
                    case MeshType.TorsoAdd:
                        return string.Format(AssetsConstants.MeshPathTemplate, characterRace, meshType);
                }

                return null;
            }
        }
    }
}
#endif