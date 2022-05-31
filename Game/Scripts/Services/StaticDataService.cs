using System.Collections.Generic;
using System.Linq;
using CharacterEditor.StaticData;
using UnityEngine;


namespace CharacterEditor.Services
{
    public class StaticDataService : IStaticDataService
    {
        private const string GAME_STATIC_DATA_PATH = "StaticData/GameData";
        private const string LEVEL_STATIC_DATA_PATH = "StaticData/Levels";

        public GameStaticData GameData => _gameData;
        public LoaderType LoaderType => _gameData.LoaderType;
        public MeshAtlasType MeshAtlasType => _gameData.MeshAtlasType;

        private GameStaticData _gameData;
        private Dictionary<string, LevelStaticData> _levels;

        public void LoadData()
        {
            _gameData = Resources.Load<GameStaticData>(GAME_STATIC_DATA_PATH);
            LoadLevels();
        }

        private void LoadLevels()
        {
            _levels = Resources
                .LoadAll<LevelStaticData>(LEVEL_STATIC_DATA_PATH)
                .ToDictionary(x => x.LevelKey, x => x);
        }

        public LevelStaticData ForLevel(string sceneKey) =>
            _levels.TryGetValue(sceneKey, out var levelData)
                ? levelData
                : null;

    }
}