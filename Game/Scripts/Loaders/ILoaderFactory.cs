using System.Threading.Tasks;
using CharacterEditor.CharacterInventory;
using UnityEngine;

namespace CharacterEditor.Services
{
    public interface ILoaderFactory : IService
    {
        ITextureLoader CreateTextureLoader();
        IMeshLoader CreateMeshLoader();
        IConfigLoader CreateConfigLoader();
        ISpriteLoader CreateIconLoader();
        ICursorLoader CreateCursorLoader();
        IDataLoader<ItemData> CreateItemLoader();
        IDataLoader<PlayableNpcConfig> CreatePlayerCharacterLoader();
        IDataLoader<EnemyConfig> CreateEnemyLoader();
        IDataLoader<ContainerConfig> CreateContainerLoader();
        ICommonLoader<GameObject> CreateGameObjectLoader();
        ICommonLoader<Material> CreateMaterialLoader();
        IPathDataProvider CreateDataPathProvider();
        Task Prepare();
    }
}
