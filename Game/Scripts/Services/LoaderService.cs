
using System.Threading.Tasks;
using Assets.Game.Scripts.Loaders;
using CharacterEditor.CharacterInventory;
using Game;
using UnityEngine;

namespace CharacterEditor.Services
{
    class LoaderService : ILoaderService
    {
        private readonly ICoroutineRunner _coroutineRunner;
        private ILoaderFactory _loaderFactory;

        public IDataManager DataManager { get; private set; }
        public IMeshLoader MeshLoader { get; }
        public ITextureLoader TextureLoader { get; }
        public IConfigLoader ConfigLoader { get; }
        public IIconLoader IconLoader { get; }
        public IDataLoader<ItemData> ItemLoader { get; }
        public IDataLoader<PlayerCharacterConfig> PlayerCharacterLoader { get; }
        public IDataLoader<EnemyConfig> EnemyLoader { get; }
        public IDataLoader<ContainerConfig> ContainerLoader { get; }
        public ICommonLoader<GameObject> GameObjectLoader { get; }

        public LoaderService(IStaticDataService staticDataService, ICoroutineRunner coroutineRunner)
        {
            _coroutineRunner = coroutineRunner;
            Initialize(staticDataService.LoaderType);

            MeshLoader = _loaderFactory.CreateMeshLoader(staticDataService.MeshAtlasType);
            TextureLoader = _loaderFactory.CreateTextureLoader();
            ConfigLoader = _loaderFactory.CreateConfigLoader();
            IconLoader = _loaderFactory.CreateIconLoader();
            ItemLoader = _loaderFactory.CreateItemLoader();
            PlayerCharacterLoader = _loaderFactory.CreatePlayerCharacterLoader();
            EnemyLoader = _loaderFactory.CreateEnemyLoader();
            ContainerLoader = _loaderFactory.CreateContainerLoader();
            GameObjectLoader = _loaderFactory.CreateGameObjectLoader();
        }

        private void Initialize(LoaderType loaderType)
        {
            switch (loaderType)
            {
#if UNITY_EDITOR
                case LoaderType.AssetDatabase:
                    _loaderFactory = new AssetDatabaseLoader.AssetDatabaseLoaderFactory();
                    DataManager = new DataManager("assetBundleInfo");
                    break;
#endif
                // case LoaderType.Addresable:
                // _loaderFactory = new Addre
                default:
                    _loaderFactory= new AssetBundleLoader.AssetBundleLoaderFactory(_coroutineRunner);
                    DataManager = new DataManager("assetBundleInfo");

                    break;
            }
        }

        public Task Initialize()
        {
            return _loaderFactory.Prepare();
        }
    }
}