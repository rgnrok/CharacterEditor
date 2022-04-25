using CharacterEditor.StaticData;
using UnityEngine;


namespace CharacterEditor.Services
{
    class StaticDataService : IStaticDataService
    {
        private const string GAME_STATIC_DATA = "StaticData/LoaderData";

        public LoaderType LoaderType => _staticData.LoaderType;
        public MeshAtlasType MeshAtlasType => _staticData.MeshAtlasType;

        private LoaderStaticData _staticData;

        public void LoadData()
        {
            _staticData = Resources.Load<LoaderStaticData>(GAME_STATIC_DATA);
        }
    }
}