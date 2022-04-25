using System.Threading.Tasks;
using CharacterEditor.AssetBundleLoader;
using CharacterEditor.CharacterInventory;
using UnityEngine;

namespace CharacterEditor.Services
{
    public interface ILoaderService : IService
    {
        IMeshLoader MeshLoader { get; }
        ITextureLoader TextureLoader { get; }
        IConfigLoader ConfigLoader { get; }
        IIconLoader IconLoader { get; }
        IDataLoader<ItemData> ItemLoader { get; }
        IDataLoader<PlayerCharacterConfig> PlayerCharacterLoader { get; }
        IDataLoader<EnemyConfig> EnemyLoader { get; }
        IDataLoader<ContainerConfig> ContainerLoader { get; }
        ICommonLoader<GameObject> GameObjectLoader { get; }
        Task Initialize();
    }
}