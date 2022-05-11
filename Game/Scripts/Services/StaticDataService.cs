using System.Collections.Generic;
using System.Linq;
using CharacterEditor.StaticData;
using UnityEngine;


namespace CharacterEditor.Services
{
    class StaticDataService : IStaticDataService
    {
        private const string GAME_STATIC_DATA_PATH = "StaticData/LoaderData";
        private const string LEVEL_STATIC_DATA_PATH = "StaticData/Levels";

        public LoaderType LoaderType => _loaderData.LoaderType;
        public MeshAtlasType MeshAtlasType => _loaderData.MeshAtlasType;

        private LoaderStaticData _loaderData;
        private Dictionary<string, LevelStaticData> _levels;

        public void LoadData()
        {
            _loaderData = Resources.Load<LoaderStaticData>(GAME_STATIC_DATA_PATH);
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