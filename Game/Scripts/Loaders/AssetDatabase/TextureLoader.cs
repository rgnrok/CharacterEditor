#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace CharacterEditor
{
    namespace AssetDatabaseLoader
    {
        /*
         * Parse and Load Meshes (Only editor)
         */
        public class TextureLoader : CommonLoader<Texture2D>, ITextureLoader
        {
            public string[][] ParseTextures(string rootPath)
            {
                return Parse(rootPath);
            }

            public string[][] ParseCharacterTextures(string race, TextureType textureType)
            {
                return Parse(GetFolderPath(race, textureType));
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

            private string[][] Parse(string path)
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
                    var textures = AssetDatabase.FindAssets("t:texture2D", new string[] {path});
                    texturePaths = new string[textures.Length][];

                    for (int i = 0; i < textures.Length; i++)
                        texturePaths[i] = new string[] {AssetDatabase.GUIDToAssetPath(textures[i])};
                }
                return texturePaths;
            }
        }
    }
}
#endif