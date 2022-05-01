using System.Threading.Tasks;
using CharacterEditor.CharacterInventory;
using CharacterEditor.Services;
using UnityEngine;

#if UNITY_EDITOR
namespace CharacterEditor
{
    namespace AssetDatabaseLoader
    {
        public class AssetDatabaseLoaderFactory : ILoaderFactory
        {
            public IMeshLoader CreateMeshLoader(MeshAtlasType atlasType)
            {
                return new MeshLoader(new TextureLoader(), atlasType);
            }

            public ITextureLoader CreateTextureLoader()
            {
                return new TextureLoader();
            }

            public IConfigLoader CreateConfigLoader()
            {
                return new ConfigLoader();
            }

            public ISpriteLoader CreateIconLoader()
            {
                return new SpriteLoader();
            }

            public IDataLoader<ItemData> CreateItemLoader()
            {
                return new ItemLoader();
            }

            public IDataLoader<PlayerCharacterConfig> CreatePlayerCharacterLoader()
            {
                return new PlayerCharacterLoader();
            }

            public IDataLoader<EnemyConfig> CreateEnemyLoader()
            {
                return new EnemyLoader();
            }

            public IDataLoader<ContainerConfig> CreateContainerLoader()
            {
                return new ContainerLoader();
            }

            public ICommonLoader<GameObject> CreateGameObjectLoader()
            {
                return new CommonLoader<GameObject>();
            }

            public Task Prepare()
            {
                return null;
            }
        }
    }
}
#endif