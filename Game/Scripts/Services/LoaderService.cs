using System.Threading.Tasks;
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
        public ISpriteLoader SpriteLoader { get; }
        public IDataLoader<ItemData> ItemLoader { get; }
        public IDataLoader<PlayableNpcConfig> PlayableNpcLoader { get; }
        public IDataLoader<EnemyConfig> EnemyLoader { get; }
        public IDataLoader<ContainerConfig> ContainerLoader { get; }
        public ICommonLoader<GameObject> GameObjectLoader { get; }
        public ICommonLoader<Material> MaterialLoader { get; }
        public IDataPathProvider DataPathProvider { get; }

        public LoaderService(IStaticDataService staticDataService, ICoroutineRunner coroutineRunner)
        {
            _coroutineRunner = coroutineRunner;
            Initialize(staticDataService.LoaderType, staticDataService.MeshAtlasType);

            MeshLoader = _loaderFactory.CreateMeshLoader();
            TextureLoader = _loaderFactory.CreateTextureLoader();
            ConfigLoader = _loaderFactory.CreateConfigLoader();
            SpriteLoader = _loaderFactory.CreateIconLoader();
            ItemLoader = _loaderFactory.CreateItemLoader();
            PlayableNpcLoader = _loaderFactory.CreatePlayerCharacterLoader();
            EnemyLoader = _loaderFactory.CreateEnemyLoader();
            ContainerLoader = _loaderFactory.CreateContainerLoader();
            GameObjectLoader = _loaderFactory.CreateGameObjectLoader();
            MaterialLoader = _loaderFactory.CreateMaterialLoader();
            DataPathProvider = _loaderFactory.CreateDataPathProvider();
        }

        private void Initialize(LoaderType loaderType, MeshAtlasType meshAtlasType)
        {
            switch (loaderType)
            {
#if UNITY_EDITOR
                case LoaderType.AssetDatabase:
                    _loaderFactory = new AssetDatabaseLoader.AssetDatabaseLoaderFactory();
                    DataManager = new AssetDatabaseLoader.DataManager(meshAtlasType);
                    break;
#endif
                // case LoaderType.Addresable:
                // _loaderFactory = new Addre
                default:
                    DataManager = new AssetBundleLoader.DataManager("assetBundleInfo");
                    _loaderFactory= new AssetBundleLoader.AssetBundleLoaderFactory((AssetBundleLoader.DataManager) DataManager, _coroutineRunner);

                    break;
            }
        }

        public Task Initialize()
        {
            return _loaderFactory.Prepare();
        }
    }
}