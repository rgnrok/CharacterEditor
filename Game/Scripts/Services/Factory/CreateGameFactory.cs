using System.Threading.Tasks;
using CharacterEditor.Mesh;
using UnityEngine;

namespace CharacterEditor.Services
{
    public class CreateGameFactory : ICreateGameFactory, IMeshInstanceCreator
    {
        private readonly ILoaderService _loaderService;

        public CreateGameFactory(ILoaderService loaderService)
        {
            _loaderService = loaderService;
        }

        public async Task<CharacterGameObjectData> SpawnCreateCharacter(CharacterConfig config)
        {
            if (config.CreateGamePrefab == null)
            {
                var prefabPath = _loaderService.PathDataProvider.GetPath(config.createGamePrefabPath);
                config.CreateGamePrefab = await _loaderService.GameObjectLoader.LoadByPath(prefabPath);
            }

            var prefabInstance = Object.Instantiate(config.CreateGamePrefab);
            prefabInstance.SetActive(false);

            var gameObjectData = new CharacterGameObjectData(config, prefabInstance);

            return gameObjectData;
        }

        public GameObject CreateMeshInstance(CharacterMesh characterMesh, Transform anchor)
        {
            if (characterMesh.LoadedMeshObject == null) return null;

            return Object.Instantiate(characterMesh.LoadedMeshObject, anchor.position, anchor.rotation, anchor);
        }
    }
}