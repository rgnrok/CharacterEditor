using System;
using System.Threading.Tasks;
using CharacterEditor.CharacterInventory;
using EnemySystem;
using UnityEngine;

namespace CharacterEditor.Services
{
    public interface IGameFactory : IService, ICleanable
    {
        Task<Character> CreateGameCharacter(CharacterSaveData characterData, CharacterConfig config,
            Texture2D skinTexture, Texture2D faceTexture, Vector3 position);

        Task<Character> CreatePlayableNpc(PlayableNpcConfig config, Texture2D skinTexture, Texture2D faceTexture, Sprite portraitIcon, Vector3 position);

        Task<Enemy> CreateEnemy(string guid, EnemyConfig config, Material material, Texture2D skinTexture,
            Texture2D faceTexture, Texture2D armorTexture, Sprite portraitIcon, Vector3 position);

        Task<Container> CreateContainer(ContainerConfig config, ContainerSaveData containerSaveData, Vector3 position);

        event Action<Character> OnCharacterSpawned;
    }
}