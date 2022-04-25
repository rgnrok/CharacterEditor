
using System.Threading.Tasks;
using CharacterEditor.CharacterInventory;
using Game;
using UnityEngine;

namespace CharacterEditor.Services
{
    class LoaderService : ILoaderService
    {
        private readonly ICoroutineRunner _coroutineRunner;
        private readonly ILoaderFactory _loaderFactory;

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
            _loaderFactory = CreateFactory(staticDataService.LoaderType);

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

        private ILoaderFactory CreateFactory(LoaderType loaderType)
        {
            switch (loaderType)
            {
#if UNITY_EDITOR
                case LoaderType.AssetDatabase:
                    return new AssetDatabaseLoader.AssetDatabaseLoaderFactory();
#endif
                // case LoaderType.Addresable:
                // _loaderFactory = new Addre
                default:
                    return new AssetBundleLoader.AssetBundleLoaderFactory(_coroutineRunner);
            }
        }

        public Task Initialize()
        {
            return _loaderFactory.Prepare();
        }
    }
}