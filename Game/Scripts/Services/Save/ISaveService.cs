using System.Collections.Generic;
using CharacterEditor.Mesh;
using UnityEngine;

namespace CharacterEditor.Services
{
    public interface ISaveService : IService
    {
        string[] GetSaves();

        void CreateGame(string saveName, string characterGuid,
            Texture2D characterTexture, Sprite portrait,
            IEnumerable<CharacterMesh> faceMeshes, Texture2D faceMeshTexture);

        void SaveGame(string saveName, string levelName, GameManager gameManager);

        string GetLastSave();
    }
}