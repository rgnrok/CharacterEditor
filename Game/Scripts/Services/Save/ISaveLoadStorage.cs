using UnityEngine;

namespace CharacterEditor
{
    public interface ISaveLoadStorage : IService
    {
        bool SaveData(string saveName, SaveData saveData);
        SaveData LoadData(string saveName);
        bool IsSaveExist(string saveName);
        string[] GetSaves();
        bool SaveCharacterTexture(string saveName, string characterGuid, string textureName, Texture2D texture);
        Texture2D LoadCharacterTexture(string saveName, string characterGuid, string textureName, int size);
    }
}