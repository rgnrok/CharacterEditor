using System.IO;
using System.Threading.Tasks;
using AssetBundles;
using CharacterEditor.CharacterInventory;
using CharacterEditor.Services;
using Game;
using UnityEngine;

namespace CharacterEditor
{
    namespace AssetBundleLoader
    {
        public class AssetBundleLoaderFactory : ILoaderFactory
        {
            private readonly ICoroutineRunner _coroutineRunner;
            private readonly RemoteDataManager _dataManager;

            public AssetBundleLoaderFactory(RemoteDataManager dataManager, ICoroutineRunner coroutineRunner)
            {
                _coroutineRunner = coroutineRunner;
                _dataManager = dataManager;
            }

            public IMeshLoader CreateMeshLoader() => 
                new MeshLoader(CreateTextureLoader(), _coroutineRunner);

            public ITextureLoader CreateTextureLoader() => 
                new TextureLoader(_coroutineRunner);

            public IConfigLoader CreateConfigLoader() => 
                new ConfigLoader(_coroutineRunner, _dataManager);

            public ISpriteLoader CreateIconLoader() => 
                new SpriteLoader(_coroutineRunner);

            public ICursorLoader CreateCursorLoader() =>
                new CursorLoader(CreateTextureLoader());

            public IDataLoader<ItemData> CreateItemLoader() => 
                new DataLoader<ItemData>(_dataManager.Items, new CommonLoader<ItemData>(_coroutineRunner));

            public IDataLoader<PlayableNpcConfig> CreatePlayerCharacterLoader() => 
                new DataLoader<PlayableNpcConfig>(_dataManager.PlayerCharacters, new CommonLoader<PlayableNpcConfig>(_coroutineRunner));

            public IDataLoader<EnemyConfig> CreateEnemyLoader() => 
                new DataLoader<EnemyConfig>(_dataManager.Enemies, new CommonLoader<EnemyConfig>(_coroutineRunner));

            public IDataLoader<ContainerConfig> CreateContainerLoader() => 
                new DataLoader<ContainerConfig>(_dataManager.Containers, new CommonLoader<ContainerConfig>(_coroutineRunner));

            public ICommonLoader<GameObject> CreateGameObjectLoader() => 
                new CommonLoader<GameObject>(_coroutineRunner);

            public ICommonLoader<Material> CreateMaterialLoader() => 
                new CommonLoader<Material>(_coroutineRunner);

            public IPathDataProvider CreateDataPathProvider() => 
                new PathDataProvider();

            public async Task Prepare()
            {
                await InitializeAssetBundles();
                AssetBundleManager.ActiveVariants = null;
            }

            // Initialize the downloading url and AssetBundleManifest object.
            private async Task InitializeAssetBundles()
            {
#if UNITY_EDITOR
                // AssetBundleManager.SetDevelopmentAssetBundleServer();
                var absolutePath = Path.Combine(Application.streamingAssetsPath, Utility.AssetBundlesOutputPath) + "/";
                AssetBundleManager.SetSourceAssetBundleURL(absolutePath);
#else

             AssetBundleManager.SetSourceAssetBundleURL(Path.Combine(Application.streamingAssetsPath, Utility.AssetBundlesOutputPath) + "/");
            // Or customize the URL based on your deployment or configuration
            //AssetBundleManager.SetSourceAssetBundleURL("http://www.MyWebsite/MyAssetBundles");
#endif

#if UNITY_EDITOR
                if (AssetBundleManager.SimulateAssetBundleInEditor)
                {
                    AssetBundleManager.Initialize();
                    return;
                }
#endif
                // Initialize AssetBundleManifest which loads the AssetBundleManifest object.
                var request = AssetBundleManager.Initialize();

                while (!request.IsDone())
                    await Task.Yield();

                while (request.GetAsset<AssetBundleManifest>() == null)
                    await Task.Yield();

            }
        }
    }
}