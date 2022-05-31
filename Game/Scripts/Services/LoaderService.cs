using System.Threading.Tasks;
using CharacterEditor.CharacterInventory;
using Game;
using UnityEngine;

namespace CharacterEditor.Services
{
    public class LoaderService : ILoaderService
    {
        private readonly ICoroutineRunner _coroutineRunner;
        private readonly ILoaderFactory _loaderFactory;

        public IDataManager DataManager { get; private set; }
        public IMeshLoader MeshLoader { get; }
        public ITextureLoader TextureLoader { get; }
        public IConfigLoader ConfigLoader { get; }
        public ISpriteLoader SpriteLoader { get; }
        public ICursorLoader CursorLoader { get; }
        public IDataLoader<ItemData> ItemLoader { get; }
        public IDataLoader<PlayableNpcConfig> PlayableNpcLoader { get; }
        public IDataLoader<EnemyConfig> EnemyLoader { get; }
        public IDataLoader<ContainerConfig> ContainerLoader { get; }
        public ICommonLoader<GameObject> GameObjectLoader { get; }
        public ICommonLoader<Material> MaterialLoader { get; }
        public IPathDataProvider PathDataProvider { get; }

        public Task Initialize()
        {
            return _loaderFactory.Prepare();
        }

        public void CleanUp()
        {
            MeshLoader.CleanUp();
            TextureLoader.CleanUp();
            ConfigLoader.CleanUp();
            SpriteLoader.CleanUp();
            CursorLoader.CleanUp();
            ItemLoader.CleanUp();
            PlayableNpcLoader.CleanUp();
            EnemyLoader.CleanUp();
            ContainerLoader.CleanUp();
            GameObjectLoader.CleanUp();
            MaterialLoader.CleanUp();
        }

        public LoaderService(IStaticDataService staticDataService, ICoroutineRunner coroutineRunner)
        {
            _coroutineRunner = coroutineRunner;
            _loaderFactory = InitLoaderFactory(staticDataService.LoaderType, staticDataService.MeshAtlasType);

            MeshLoader = _loaderFactory.CreateMeshLoader();
            TextureLoader = _loaderFactory.CreateTextureLoader();
            ConfigLoader = _loaderFactory.CreateConfigLoader();
            SpriteLoader = _loaderFactory.CreateIconLoader();
            CursorLoader = _loaderFactory.CreateCursorLoader();
            ItemLoader = _loaderFactory.CreateItemLoader();
            PlayableNpcLoader = _loaderFactory.CreatePlayerCharacterLoader();
            EnemyLoader = _loaderFactory.CreateEnemyLoader();
            ContainerLoader = _loaderFactory.CreateContainerLoader();
            GameObjectLoader = _loaderFactory.CreateGameObjectLoader();
            MaterialLoader = _loaderFactory.CreateMaterialLoader();
            PathDataProvider = _loaderFactory.CreateDataPathProvider();
        }

        private ILoaderFactory InitLoaderFactory(LoaderType loaderType, MeshAtlasType meshAtlasType)
        {
            switch (loaderType)
            {
#if UNITY_EDITOR
                case LoaderType.AssetDatabase:
                    DataManager = new AssetDatabaseLoader.DataManager(meshAtlasType);
                    return new AssetDatabaseLoader.AssetDatabaseLoaderFactory();
#endif
                case LoaderType.Addresable:
                    DataManager = new RemoteDataManager("addressablesInfo");
                    return new AddressableLoader.AddressableLoaderFactory((RemoteDataManager)DataManager);
                default:
                    DataManager = new RemoteDataManager("assetBundleInfo");
                    return new AssetBundleLoader.AssetBundleLoaderFactory((RemoteDataManager) DataManager, _coroutineRunner);
            }
        }
    }
}