using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CharacterEditor.CharacterInventory;
using EnemySystem;
using StatSystem;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CharacterEditor.Services
{
    public class GameFactory : IGameFactory
    {
        private readonly ILoaderService _loaderService;
        private readonly ICharacterEquipItemService _equipItemService;

        public event Action<Character> OnCharacterSpawned;

        private HashSet<ICleanable> _cleanableEntities = new HashSet<ICleanable>();

        public GameFactory(ILoaderService loaderService, ICharacterEquipItemService equipItemService)
        {
            _loaderService = loaderService;
            _equipItemService = equipItemService;
        }

        public void CleanUp()
        {
            foreach (var cleanable in _cleanableEntities)
                cleanable.CleanUp();

            _cleanableEntities.Clear();
        }

        public async Task<Character> CreateGameCharacter(CharacterSaveData characterData, CharacterConfig config, Texture2D skinTexture, Texture2D faceTexture, Vector3 position)
        {
            var portraitIcon = await _loaderService.SpriteLoader.LoadPortrait(characterData.portrait);

            if (config.PreviewPrefab == null)
                config.PreviewPrefab = await LoadConfigPrefab(config.previewPrefabPath);
            if (config.Prefab == null)
                config.Prefab = await LoadConfigPrefab(config.prefabPath);

            //Setup config
            var previewInstance = Object.Instantiate(config.PreviewPrefab);
            previewInstance.transform.position = Vector3.zero;
            previewInstance.SetActive(false);

            var characterInstance = Object.Instantiate(config.Prefab, position, characterData.rotation);
            characterInstance.SetActive(false);

            var gameObjectData = new CharacterGameObjectData(config, characterInstance, previewInstance);
            var character = new Character(characterData, gameObjectData, skinTexture, faceTexture, portraitIcon);
            character.Init();

            var itemGuids = new List<string>();
            foreach (var pair in characterData.equipItems)
                itemGuids.Add(pair.Value.dataGuid);

            foreach (var item in characterData.inventoryCells.Values)
                itemGuids.Add(item.dataGuid);


            var items = await _loaderService.ItemLoader.LoadData(itemGuids);
            // Load item textures and meshes
            var equipItems = new Dictionary<EquipItemSlot, EquipItem>();
            var faceMeshItems = new Dictionary<MeshType, FaceMesh>();

            //todo add in inventory only equipItems!
            foreach (var ceilPair in characterData.inventoryCells)
            {
                var itemData = ceilPair.Value;

                if (!items.TryGetValue(itemData.dataGuid, out var data)) continue;
                if (!(data is EquipItemData equipItemData)) continue;

                var eiMesh = new EquipItemMesh(equipItemData, _loaderService.TextureLoader, _loaderService.MeshLoader, _loaderService.PathDataProvider);
                var equipItem = new EquipItem(itemData.guid, equipItemData, eiMesh, itemData.stats);

                GameManager.Instance.Inventory.SetItemToInventory(characterData.guid, equipItem, ceilPair.Key);
            }

            // Equip items from inventory
            foreach (var pair in characterData.equipItems)
            {
                if (!items.TryGetValue(pair.Value.dataGuid, out var data)) continue;
                if (!(data is EquipItemData equipItemData)) continue;

                var eiMesh = new EquipItemMesh(equipItemData, _loaderService.TextureLoader, _loaderService.MeshLoader, _loaderService.PathDataProvider);
                equipItems[pair.Key] = new EquipItem(pair.Value.guid, equipItemData, eiMesh, pair.Value.stats);
            }

            foreach (var pair in characterData.faceMeshItems)
                faceMeshItems[pair.Key] = FaceMeshFactory.Create(pair.Key, pair.Value, _loaderService.MeshLoader);

            await EquipItems(character, equipItems, faceMeshItems);

            OnCharacterSpawned?.Invoke(character);
            _cleanableEntities.Add(character);

            return character;
        }

        public async Task<Character> CreatePlayableNpc(PlayableNpcConfig config, Texture2D skinTexture, Texture2D faceTexture, Sprite portraitIcon, Vector3 position)
        {
            if (config.characterConfig.Prefab == null)
                config.characterConfig.Prefab = await LoadConfigPrefab(config.characterConfig.prefabPath);
            if (config.characterConfig.PreviewPrefab == null)
                config.characterConfig.PreviewPrefab = await LoadConfigPrefab(config.characterConfig.previewPrefabPath);

            var go = Object.Instantiate(config.characterConfig.Prefab, position, Quaternion.identity);
            go.layer = Constants.LAYER_NPC;

            var goData = new CharacterGameObjectData(config.characterConfig, go, null);
            var stats = new DefaultStatCollection(); //tmp load from config in feature
            var character = new Character(config.guid, stats, goData, skinTexture, faceTexture, portraitIcon);
            character.Init();

            var equipItems = new Dictionary<EquipItemSlot, EquipItem>();
            var faceMeshItems = new Dictionary<MeshType, FaceMesh>();
            foreach (var itemInfo in config.equipItems)
            {
                var eiMesh = new EquipItemMesh(itemInfo.item, _loaderService.TextureLoader, _loaderService.MeshLoader, _loaderService.PathDataProvider);
                equipItems[itemInfo.itemSlot] = new EquipItem(itemInfo.item, eiMesh);
            }

            foreach (var faceMesh in config.faceMeshs)
                faceMeshItems[faceMesh.meshType] = FaceMeshFactory.Create(faceMesh.meshType, _loaderService.PathDataProvider.GetPath(faceMesh.meshPath), _loaderService.MeshLoader);

            await EquipItems(character, equipItems, faceMeshItems);
            _cleanableEntities.Add(character);

            return character;
        }

        public async Task<Enemy> CreateEnemy(string guid, EnemyConfig config, Material material, Texture2D skinTexture,
            Texture2D faceTexture, Texture2D armorTexture, Sprite portraitIcon, Vector3 position)
        {
            if (config.Prefab == null)
                config.Prefab = await LoadConfigPrefab(config.prefabPath);

            var go = Object.Instantiate(config.Prefab, position, Quaternion.identity);
            go.layer = Constants.LAYER_ENEMY;

            var goData = new EnemyGameObjectData(config, go);

            var armorMaterial = new Material(material);
            var faceMaterial = new Material(material);
            var skinMaterial = new Material(material);
            armorMaterial.mainTexture = armorTexture;
            faceMaterial.mainTexture = faceTexture;
            skinMaterial.mainTexture = skinTexture;

            foreach (var skinMesh in goData.SkinMeshes) skinMesh.material = skinMaterial;

            var stats = new DefaultStatCollection(); //tmp load from config in feature
            var enemy = new Enemy(guid, stats, goData, null, portraitIcon);
            enemy.Init();

            var prefabPaths = new List<string>();
            foreach (var bone in config.prefabBoneData.armorBones)
                prefabPaths.Add(_loaderService.PathDataProvider.GetPath(bone.prefabPath));
            foreach (var bone in config.prefabBoneData.faceBones)
                prefabPaths.Add(_loaderService.PathDataProvider.GetPath(bone.prefabPath));

            var prefabs = await _loaderService.GameObjectLoader.LoadByPath(prefabPaths);

            InstantiateItemsToBones(config.prefabBoneData.armorBones, prefabs, go, armorMaterial);
            InstantiateItemsToBones(config.prefabBoneData.faceBones, prefabs, go, faceMaterial);

            var moveComponent = enemy.EntityGameObject.GetComponent<PlayerMoveComponent>();
            if (moveComponent != null) moveComponent.Stop(true);
            _cleanableEntities.Add(enemy);

            return enemy;
        }

        public async Task<Container> CreateContainer(ContainerConfig config, ContainerSaveData containerSaveData,
            Vector3 position)
        {
            var containerGo = await _loaderService.GameObjectLoader.LoadByPath(_loaderService.PathDataProvider.GetPath(config.prefab));
            var containerInstance = Object.Instantiate(containerGo, position, Quaternion.identity);

            var container = containerInstance?.GetComponent<Container>();
            if (container == null) return null;

            if (containerSaveData == null)
                FillContainer(config, container);
            else
                await FillContainer(containerSaveData, container);

            return container;
        }

        private async Task<GameObject> LoadConfigPrefab(PathData prefabPathData)
        {
            var prefabPath = _loaderService.PathDataProvider.GetPath(prefabPathData);
            return await _loaderService.GameObjectLoader.LoadByPath(prefabPath);
        }

        private async Task FillContainer(ContainerSaveData containerSaveData, Container container)
        {
            var itemGuids = new List<string>(containerSaveData.items.Count);
            foreach (var itemData in containerSaveData.items)
                itemGuids.Add(itemData.Value.dataGuid);

            var itemsData = await _loaderService.ItemLoader.LoadData(itemGuids);
            var containerItems = new Dictionary<int, ItemData>();
            foreach (var itemDataPair in containerSaveData.items)
            {
                if (!itemsData.TryGetValue(itemDataPair.Value.dataGuid, out var itemData)) continue;
                containerItems[itemDataPair.Key] = itemData;
            }

            container.SetData(containerSaveData, containerItems);
        }

        private static void FillContainer(ContainerConfig config, Container container)
        {
            var items = new Dictionary<int, ItemData>();
            var containerSaveData = new ContainerSaveData {guid = config.guid};

            for (var i = 0; i < config.initItems.Length; i++)
                items[i] = config.initItems[i];

            container.SetData(containerSaveData, items);
        }


        private void InstantiateItemsToBones(PrefabBoneData.BoneData[] bones, Dictionary<string, GameObject> objects, GameObject go,
            Material armorMaterial)
        {
            foreach (var bone in bones)
            {
                if (!objects.TryGetValue(_loaderService.PathDataProvider.GetPath(bone.prefabPath), out var prefabGo)) continue;

                var rootBone = go.transform.FindTransform(bone.bone);
                if (rootBone == null) continue;

                var meshObject = Object.Instantiate(prefabGo, rootBone.transform.position, rootBone.transform.rotation, rootBone);
                foreach (var meshRenderer in meshObject.GetComponentsInChildren<MeshRenderer>())
                    meshRenderer.material = armorMaterial;
            }
        }


        private async Task EquipItems(Character character, Dictionary<EquipItemSlot, EquipItem> equipItems, Dictionary<MeshType, FaceMesh> faceMeshItems)
        {
            foreach (var faceMeshPair in faceMeshItems.Values)
            {
                await faceMeshPair.LoadTextureAndMesh();
                character.AddFaceMesh(faceMeshPair);
            }

            _equipItemService.SetupCharacter(character);
            await _equipItemService.EquipItems(character, equipItems);
        }
    }
}