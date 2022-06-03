using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CharacterEditor.Mesh;
using CharacterEditor.StaticData;
using EnemySystem;
using UnityEngine;

namespace CharacterEditor.Services
{
    public interface ISaveLoadService : IService
    {
        string[] GetSaves();

        void CreateGame(string saveName, string characterGuid,
            Texture2D characterTexture, Sprite portrait,
            IEnumerable<CharacterMesh> faceMeshes, Texture2D faceMeshTexture);

        void SaveGame(string saveName, string levelName, GameManager gameManager);

        string GetLastSave();

        Task<bool> Load(string saveName, LevelStaticData levelData, Action<int> loadProcessAction);

        event Action<SaveData> OnLoadData;
        event Action<IList<Character>> OnCharactersLoaded;
        event Action<IList<Character>> OnPlayableNpcLoaded;
        event Action<IList<Enemy>> OnEnemiesLoaded;
    }
}