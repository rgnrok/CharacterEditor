using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CharacterEditor.StaticData;
using EnemySystem;

namespace CharacterEditor.Services
{
    public interface ILoadSaveService : IService
    {
        Task<bool> Load(string saveName, LevelStaticData levelData, Action<int> loadProcessAction);

        event Action<SaveData> OnLoadData;
        event Action<IList<Character>> OnCharactersLoaded;
        event Action<IList<Character>> OnPlayableNpcLoaded;
        event Action<IList<Enemy>> OnEnemiesLoaded;
    }
}