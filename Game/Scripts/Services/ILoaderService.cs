using System.Threading.Tasks;
using CharacterEditor.CharacterInventory;
using UnityEngine;

namespace CharacterEditor.Services
{
    public interface ILoaderService : IService, ICleanable
    {
        IDataManager DataManager { get; }
        IMeshLoader MeshLoader { get; }
        ITextureLoader TextureLoader { get; }
        IConfigLoader ConfigLoader { get; }
        ISpriteLoader SpriteLoader { get; }
        IDataLoader<ItemData> ItemLoader { get; }
        IDataLoader<PlayableNpcConfig> PlayableNpcLoader { get; }
        IDataLoader<EnemyConfig> EnemyLoader { get; }
        IDataLoader<ContainerConfig> ContainerLoader { get; }
        ICommonLoader<GameObject> GameObjectLoader { get; }
        ICommonLoader<Material> MaterialLoader { get; }
        IPathDataProvider PathDataProvider { get; }
        Task Initialize();
    }
}