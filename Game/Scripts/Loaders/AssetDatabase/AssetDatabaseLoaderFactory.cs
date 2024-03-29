﻿using System.Threading.Tasks;
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
            public IMeshLoader CreateMeshLoader() => 
                new MeshLoader();

            public ITextureLoader CreateTextureLoader() => 
                new TextureLoader();

            public IConfigLoader CreateConfigLoader() => 
                new ConfigLoader();

            public ISpriteLoader CreateIconLoader() => 
                new SpriteLoader();

            public ICursorLoader CreateCursorLoader() =>
                new CursorLoader(CreateTextureLoader());

            public IDataLoader<ItemData> CreateItemLoader() => 
                new ItemLoader();

            public IDataLoader<PlayableNpcConfig> CreatePlayerCharacterLoader() => 
                new PlayableNpcLoader();

            public IDataLoader<EnemyConfig> CreateEnemyLoader() => 
                new EnemyLoader();

            public IDataLoader<ContainerConfig> CreateContainerLoader() => 
                new ContainerLoader();

            public ICommonLoader<GameObject> CreateGameObjectLoader() => 
                new CommonLoader<GameObject>();

            public ICommonLoader<Material> CreateMaterialLoader() => 
                new CommonLoader<Material>();

            public IPathDataProvider CreateDataPathProvider() => 
                new PathDataProvider();

            public Task Prepare()
            {
                return Task.CompletedTask;
            }
        }
    }
}
#endif