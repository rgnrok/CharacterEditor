using System.Threading.Tasks;
using Game;
using UnityEngine;
using UnityEngine.AI;

namespace CharacterEditor.Services
{
    public class GameFactory : IGameFactory
    {
        private readonly IAssets _assetProvider;

        public GameFactory(IAssets assetProvider)
        {
            _assetProvider = assetProvider;
        }

        public async Task<CharacterGameObjectData> SpawnCreateCharacter(CharacterConfig config)
        {
            if (config.Prefab == null) return null;

            var textureManager = TextureManager.Instance;
            var meshManager = MeshManager.Instance;

            var characterPrefab = _assetProvider.Instantiate(config.CreateGamePrefab);

            characterPrefab.SetActive(false);
            var gameObjectData = new CharacterGameObjectData(config, characterPrefab);

            await textureManager.ApplyConfig(config, gameObjectData);
            await meshManager.ApplyConfig(config, gameObjectData);

            return gameObjectData;
        }

        public async Task<CharacterGameObjectData> SpawnGameCharacter(CharacterConfig config)
        {
            var gameObjectData = await SpawnCreateCharacter(config);

            // if (gameObjectData.CharacterObject.GetComponent<PlayerController>() == null)
            //     gameObjectData.CharacterObject.AddComponent<PlayerController>();
            //
            // if (gameObjectData.CharacterObject.GetComponent<NavMeshAgent>() != null)
            //     GameObject.Destroy(gameObjectData.CharacterObject.GetComponent<NavMeshAgent>());

            return gameObjectData;
        }
    }
}