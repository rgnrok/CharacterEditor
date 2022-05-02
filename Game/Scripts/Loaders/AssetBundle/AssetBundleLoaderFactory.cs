using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using AssetBundles;
using CharacterEditor.CharacterInventory;
using CharacterEditor.Services;
using Game;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace CharacterEditor
{
    namespace AssetBundleLoader
    {
        public class AssetBundleLoaderFactory : ILoaderFactory
        {
            private readonly ICoroutineRunner _coroutineRunner;
            private readonly DataManager _dataManager;

            public AssetBundleLoaderFactory(ICoroutineRunner coroutineRunner)
            {
                _coroutineRunner = coroutineRunner;
                _dataManager = new DataManager("assetBundleInfo");
            }

            public IMeshLoader CreateMeshLoader(MeshAtlasType atlasType) => 
                new MeshLoader(CreateTextureLoader(), _coroutineRunner);

            public ITextureLoader CreateTextureLoader() => 
                new TextureLoader(_coroutineRunner);

            public IConfigLoader CreateConfigLoader() => 
                new ConfigLoader(_dataManager);

            public ISpriteLoader CreateIconLoader() => 
                new SpriteLoader(_coroutineRunner);

            public IDataLoader<ItemData> CreateItemLoader() => 
                new DataLoader<ItemData>(_dataManager.Items, new CommonLoader<ItemData>(_coroutineRunner));

            public IDataLoader<PlayerCharacterConfig> CreatePlayerCharacterLoader() => 
                new DataLoader<PlayerCharacterConfig>(_dataManager.PlayerCharacters, new CommonLoader<PlayerCharacterConfig>(_coroutineRunner));

            public IDataLoader<EnemyConfig> CreateEnemyLoader() => 
                new DataLoader<EnemyConfig>(_dataManager.Enemies, new CommonLoader<EnemyConfig>(_coroutineRunner));

            public IDataLoader<ContainerConfig> CreateContainerLoader() => 
                new DataLoader<ContainerConfig>(_dataManager.Containers, new CommonLoader<ContainerConfig>(_coroutineRunner));

            public ICommonLoader<GameObject> CreateGameObjectLoader() => 
                new CommonLoader<GameObject>(_coroutineRunner);

            public async Task Prepare()
            {
                await InitializeAssetBundles();

                // Set active variants.
                AssetBundleManager.ActiveVariants = null;
            }

      

            // Initialize the downloading url and AssetBundleManifest object.
            protected async Task InitializeAssetBundles()
            {
                // Don't destroy this gameObject as we depend on it to run the loading script.
                //DontDestroyOnLoad(gameObject);

#if UNITY_EDITOR
                // AssetBundleManager.SetSourceAssetBundleURL(Path.Combine(Application.streamingAssetsPath, Utility.AssetBundlesOutputPath) + "/");
                AssetBundleManager.SetDevelopmentAssetBundleServer();
#else
            AssetBundleManager.SetSourceAssetBundleURL(Path.Combine(Application.streamingAssetsPath, Utility.AssetBundlesOutputPath) + "/");
            // Or customize the URL based on your deployment or configuration
            //AssetBundleManager.SetSourceAssetBundleURL("http://www.MyWebsite/MyAssetBundles");
#endif
                if (AssetBundleManager.SimulateAssetBundleInEditor)
                {
                    AssetBundleManager.Initialize();
                    return;
                }
                // Initialize AssetBundleManifest which loads the AssetBundleManifest object.
                var request = AssetBundleManager.Initialize();
                while (!request.IsDone())
                    await Task.Delay(100);

                while (request.GetAsset<AssetBundleManifest>() == null)
                    await Task.Delay(100);

            }
        }
    }
}