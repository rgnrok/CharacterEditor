using System.Collections.Generic;
using System.Threading.Tasks;
using CharacterEditor.CharacterInventory;
using Game;
using UnityEngine;
using UnityEngine.AI;

namespace CharacterEditor.Services
{
    public class GameFactory : IGameFactory
    {
        private readonly ILoaderService _loaderService;

        public GameFactory(ILoaderService loaderService)
        {
            _loaderService = loaderService;
        }

        public async Task<CharacterGameObjectData> SpawnCreateCharacter(CharacterConfig config)
        {
            if (config.Prefab == null) return null;

            var textureManager = TextureManager.Instance;
            var meshManager = MeshManager.Instance;

            var characterPrefab = Object.Instantiate(config.CreateGamePrefab);

            characterPrefab.SetActive(false);
            var gameObjectData = new CharacterGameObjectData(config, characterPrefab);

            await textureManager.ApplyConfig(config, gameObjectData);
            await meshManager.ApplyConfig(config, gameObjectData);

            return gameObjectData;
        }

        public async Task<Character> SpawnGameCharacter(CharacterSaveData characterData, CharacterConfig config, Texture2D characterTexture, Texture2D faceTexture)
        {
            await FillCharacterConfig(config);

            var portraitIcon = await _loaderService.SpriteLoader.LoadPortrait(characterData.portrait);


            //Setup config
            var previewInstance = Object.Instantiate(config.PreviewPrefab);
            previewInstance.transform.position = Vector3.zero;
            previewInstance.SetActive(false);

            var characterInstance = Object.Instantiate(config.Prefab, characterData.position, characterData.rotation);
            characterInstance.SetActive(false);


            var gameObjectData = new CharacterGameObjectData(config, characterInstance, previewInstance);
            var character = new Character(characterData, gameObjectData, characterTexture, faceTexture, portraitIcon);
            character.Init();

            var itemGuids = new List<string>();
            // Надетые айтемы
            foreach (var pair in characterData.equipItems)
                itemGuids.Add(pair.Value.dataGuid);
            // Айтемы в инвентаре
            foreach (var item in characterData.inventoryCells.Values)
                itemGuids.Add(item.dataGuid);


            var items = await _loaderService.ItemLoader.LoadData(itemGuids);
            // Load item textures and meshes
            var equipItems = new Dictionary<EquipItemSlot, EquipItem>();
            var faceMeshItems = new Dictionary<MeshType, FaceMesh>();

            //todo only equipItems
            foreach (var ceilPair in characterData.inventoryCells)
            {
                var itemData = ceilPair.Value;

                if (!items.TryGetValue(itemData.dataGuid, out var data)) continue;

                var eiMesh = new EquipItemMesh((EquipItemData) data, _loaderService.TextureLoader,
                    _loaderService.MeshLoader);
                GameManager.Instance.Inventory.SetItemToInvetory(characterData.guid,
                    new EquipItem(itemData.guid, data, eiMesh, itemData.stats), ceilPair.Key);
            }

            // Equip items from inventory
            foreach (var pair in characterData.equipItems)
            {
                if (!items.TryGetValue(pair.Value.dataGuid, out var data)) continue;
                //todo use ready created item?
                var eiMesh = new EquipItemMesh((EquipItemData) data, _loaderService.TextureLoader,
                    _loaderService.MeshLoader);
                equipItems[pair.Key] = new EquipItem(pair.Value.guid, data, eiMesh, pair.Value.stats);
            }

            foreach (var pair in characterData.faceMeshItems)
                faceMeshItems[pair.Key] = new FaceMesh(_loaderService.MeshLoader, pair.Key, pair.Value);

            await EquipItems(character, equipItems, faceMeshItems);

            return character;
        }

        private async Task FillCharacterConfig(CharacterConfig config)
        {
            if (config.Prefab == null)
            {
                var gamePrefab = await _loaderService.GameObjectLoader.LoadByPath(config.createGameBundlePrefabPath);
                config.Prefab = gamePrefab;
            }

            if (config.PreviewPrefab == null)
            {
                var previewPrefab = await _loaderService.GameObjectLoader.LoadByPath(config.previewBundlePrefabPath);
                config.PreviewPrefab = previewPrefab;
            }
        }

        //todo need remove ItemManager.Instance
        private async Task EquipItems(Character character, Dictionary<EquipItemSlot, EquipItem> equipItems, Dictionary<MeshType, FaceMesh> faceMeshItems)
        {
            foreach (var faceMeshPair in faceMeshItems.Values)
            {
                faceMeshPair.LoadTextureAndMesh(character.configGuid);
                while (!faceMeshPair.ItemMesh.IsReady) await Task.Yield();
                character.AddFaceMesh(faceMeshPair);
            }

            while (!ItemManager.Instance.IsReady) await Task.Yield();
            ItemManager.Instance.SetCharacter(character);
            ItemManager.Instance.EquipItems(equipItems);
            while (!ItemManager.Instance.IsReady) await Task.Yield();
        }
    }
}