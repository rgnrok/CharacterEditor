using System.Threading.Tasks;
using CharacterEditor.CharacterInventory;
using UnityEngine;

namespace CharacterEditor.Services
{
    public interface IGameFactory : IService
    {
        Task<CharacterGameObjectData> SpawnCreateCharacter(CharacterConfig config);

        Task<Character> SpawnGameCharacter(CharacterSaveData characterData, CharacterConfig config,
            Texture2D characterTexture, Texture2D faceTexture);
    }
}