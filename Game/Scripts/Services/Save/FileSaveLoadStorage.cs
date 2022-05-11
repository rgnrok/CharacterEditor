using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace CharacterEditor
{
    public class FileSaveLoadStorage : ISaveLoadStorage
    {
        private const string SAVE_FILE_NAME = "FileName.dat";
        private static readonly char DirSep = Path.DirectorySeparatorChar;

        private readonly string _savesFolder = $"{Application.persistentDataPath}{DirSep}Saves{DirSep}";

        public bool IsSaveExist(string saveName)
        {
            var saveFilePath = SaveFilePath(saveName);
            return File.Exists(saveFilePath);
        }

        public string[] GetSaves()
        {
            if (!TryCreateDirectoryIfNotExist(_savesFolder)) return new string[0];

            var folders = Directory.GetDirectories(_savesFolder);
            var saveNames = new string[folders.Length];
            for (var i = 0; i < saveNames.Length; i++)
            {
                var folderParts = folders[i].Split(DirSep);
                saveNames[i] = folderParts[folderParts.Length - 1];
            }
            return saveNames;
        }

        public bool SaveData(string saveName, SaveData saveData)
        {
            using (var file = File.Open($"{SaveFilePath(saveName)}", FileMode.Create))
            {
                var bf = new BinaryFormatter();
                bf.Serialize(file, saveData);
                return true;
            }
        }

        public SaveData LoadData(string saveName)
        {
            if (!IsSaveExist(saveName))
                return null;

            var saveFilePath = SaveFilePath(saveName);
            using (var file = File.Open(saveFilePath, FileMode.Open))
            {
                var bf = new BinaryFormatter();
                return (SaveData)bf.Deserialize(file);
            }
        }

        public bool SaveCharacterTexture(string saveName, string characterGuid, string textureName, Texture2D texture)
        {
            var saveFolderPath = SaveFolderPath(saveName);
            var textureDir = $"{saveFolderPath}{DirSep}{characterGuid}";
            if (!TryCreateDirectoryIfNotExist(textureDir))
                return false;

            File.WriteAllBytes($"{textureDir}{DirSep}{textureName}", texture.EncodeToPNG());
            return true;
        }

        public Texture2D LoadCharacterTexture(string saveName, string characterGuid, string textureName, int size)
        {
            var saveFolderPath = SaveFolderPath(saveName);
            var textureDir = $"{saveFolderPath}{DirSep}{characterGuid}";
            if (!Directory.Exists(textureDir))
                return null;

            try
            {
                var textureData = File.ReadAllBytes($"{textureDir}{DirSep}{textureName}");
                var characterTexture = new Texture2D(size, size, TextureFormat.RGB24, false);
                characterTexture.LoadImage(textureData);

                return characterTexture;
            }
            catch (Exception e)
            {
                Logger.LogError(e.ToString());
            }

            return null;
        }

        private bool TryCreateDirectoryIfNotExist(string dir)
        {
            if (Directory.Exists(dir)) return true;

            try
            {
                var indexOf = dir.IndexOf(_savesFolder, StringComparison.Ordinal);
                if (indexOf != 0)
                {
                    Directory.CreateDirectory(_savesFolder);
                    return true;
                }

                var dirPath = dir.Substring(0, _savesFolder.Length);
                Directory.CreateDirectory(dirPath);

                var innerDir = dir.Substring(_savesFolder.Length); ;
                foreach (var part in innerDir.Split(DirSep))
                {
                    dirPath += $"{part}{DirSep}";
                    Directory.CreateDirectory(dirPath);
                }
                return true;
            }
            catch (Exception e)
            {
                Logger.LogError(e.ToString());
            }
            return false;
        }

        private string SaveFolderPath(string saveName) =>
            $"{_savesFolder}{saveName}";

        private string SaveFilePath(string saveName) =>
            $"{SaveFolderPath(saveName)}{DirSep}{SAVE_FILE_NAME}";

    }
}