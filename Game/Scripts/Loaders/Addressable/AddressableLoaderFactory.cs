using System.Threading.Tasks;
using CharacterEditor.CharacterInventory;
using CharacterEditor.Services;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace CharacterEditor
{
    namespace AddressableLoader
    {
        public class AddressableLoaderFactory : ILoaderFactory
        {
            private readonly RemoteDataManager _dataManager;

            public AddressableLoaderFactory(RemoteDataManager dataManager)
            {
                _dataManager = dataManager;
            }

            public IMeshLoader CreateMeshLoader() =>
                new MeshLoader(CreateTextureLoader());

            public ITextureLoader CreateTextureLoader() =>
                new TextureLoader();

            public IConfigLoader CreateConfigLoader() =>
                new ConfigLoader(_dataManager);

            public ISpriteLoader CreateIconLoader() =>
                new SpriteLoader();

            public ICursorLoader CreateCursorLoader() => 
                new CursorLoader(CreateTextureLoader());

            public IDataLoader<ItemData> CreateItemLoader() =>
                new DataLoader<ItemData>(_dataManager.Items, new CommonLoader<ItemData>());

            public IDataLoader<PlayableNpcConfig> CreatePlayerCharacterLoader() =>
                new DataLoader<PlayableNpcConfig>(_dataManager.PlayerCharacters, new CommonLoader<PlayableNpcConfig>());

            public IDataLoader<EnemyConfig> CreateEnemyLoader() =>
                new DataLoader<EnemyConfig>(_dataManager.Enemies, new CommonLoader<EnemyConfig>());

            public IDataLoader<ContainerConfig> CreateContainerLoader() =>
                new DataLoader<ContainerConfig>(_dataManager.Containers, new CommonLoader<ContainerConfig>());

            public ICommonLoader<GameObject> CreateGameObjectLoader() =>
                new CommonLoader<GameObject>();

            public ICommonLoader<Material> CreateMaterialLoader() =>
                new CommonLoader<Material>();

            public IPathDataProvider CreateDataPathProvider() =>
                new PathDataProvider();

            public async Task Prepare()
            {
                await InitializeAddressable();
            }

            private async Task InitializeAddressable()
            {
                var request = Addressables.InitializeAsync();

                while (!request.IsDone)
                    await Task.Yield();
            }
        }
    }
}