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
        IDataLoader<ItemData> CreateItemLoader();
        IDataLoader<PlayableNpcConfig> CreatePlayerCharacterLoader();
        IDataLoader<EnemyConfig> CreateEnemyLoader();
        IDataLoader<ContainerConfig> CreateContainerLoader();
        ICommonLoader<GameObject> CreateGameObjectLoader();
        ICommonLoader<Material> CreateMaterialLoader();
        Task Prepare();
    }
}
