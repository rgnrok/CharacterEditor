#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace CharacterEditor
{
    namespace AssetDatabaseLoader
    {
        public class DataManager : IDataManager
        {
            private readonly MeshAtlasType _meshAtlasType;

            public DataManager(MeshAtlasType atlasType)
            {
                _meshAtlasType = atlasType;
            }

            public string[][] ParseCharacterTextures(string race, TextureType textureType)
            {
                return ParseTextures(GetFolderPath(race, textureType));
            }


            public Dictionary<string, string[][]> ParseCharacterMeshes(string race, MeshType meshType)
            {
                var folderPath = GetFolderPath(race, meshType);
                if (folderPath == null) return null;

                var dirPath = Path.Combine(Application.dataPath, folderPath.Substring(7));
                if (!Directory.Exists(dirPath)) return null;

                var folders = Directory.GetDirectories(dirPath);
                var meshAndTexturePaths = new Dictionary<string, string[][]>();

                var modelType = _meshAtlasType == MeshAtlasType.Static ? "StaticModel" : "Model";
                for (var i = 0; i < folders.Length; i++)
                {
                    var path = folders[i].Substring(Application.dataPath.Length - 6);
                    var meshGUIDs = AssetDatabase.FindAssets("t:GameObject", new[] { path + "/" + modelType });
                    var meshPath = AssetDatabase.GUIDToAssetPath(meshGUIDs[0]);

                    meshAndTexturePaths[meshPath] = ParseTextures(path + "/Textures");
                }

                return meshAndTexturePaths;
            }

            private string[][] ParseTextures(string path)
            {
                if (path == null) return new string[0][];

                var dirPath = Path.Combine(Application.dataPath, path.Substring(7));
                if (!Directory.Exists(dirPath))
                    return new string[0][];

                var folders = Directory.GetDirectories(dirPath);
                bool withColor = folders.Length != 0;

                string[][] texturePaths;
                if (withColor)
                {
                    texturePaths = new string[folders.Length][];

                    for (int i = 0; i < folders.Length; i++)
                    {
                        var textures = AssetDatabase.FindAssets("t:texture2D", new string[]
                            {
                                folders[i].Substring(Application.dataPath.Length - 6)
                            }
                        );
                        texturePaths[i] = new string[textures.Length];
                        for (int j = 0; j < textures.Length; j++)
                            texturePaths[i][j] = AssetDatabase.GUIDToAssetPath(textures[j]);
                    }
                }
                else
                {
                    var textures = AssetDatabase.FindAssets("t:texture2D", new string[] { path });
                    texturePaths = new string[textures.Length][];

                    for (int i = 0; i < textures.Length; i++)
                        texturePaths[i] = new string[] { AssetDatabase.GUIDToAssetPath(textures[i]) };
                }
                return texturePaths;
            }

            private string GetFolderPath(string characterRace, TextureType textureType)
            {
                switch (textureType)
                {
                    case TextureType.Skin:
                    case TextureType.Eye:
                    case TextureType.Eyebrow:
                    case TextureType.Beard:
                    case TextureType.Hair:
                    case TextureType.Scar:
                    case TextureType.FaceFeature:
                        return string.Format(AssetsConstants.SkinPathTemplate, characterRace, textureType);
                    case TextureType.Head:
                    case TextureType.Pants:
                    case TextureType.Torso:
                    case TextureType.Shoe:
                    case TextureType.Glove:
                    case TextureType.Belt:
                    case TextureType.RobeLong:
                    case TextureType.RobeShort:
                    case TextureType.Cloak:
                        return string.Format(AssetsConstants.ClothePathTemplate, characterRace, textureType);
                }

                return null;
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