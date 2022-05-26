using System;
using System.Collections.Generic;
using System.Linq;
using CharacterEditor;
using CharacterEditor.CharacterInventory;
using UnityEditor;
using UnityEngine;

namespace Editor
{

    [Serializable]
    public class EquipItemSlotItemData
    {
        public EquipItemSlot itemSlot;
        public EquipItemData item;

        public EquipItemSlotItemData(EquipItemSlot slot, EquipItemData item)
        {
            itemSlot = slot;
            this.item = item;
        }
    }

    [Serializable]
    public class MeshTypeFaceMeshObject
    {
        public MeshType meshType;
        public GameObject meshObject;

        public MeshTypeFaceMeshObject(MeshType type, GameObject obj)
        {
            meshType = type;
            meshObject = obj;
        }
    }

    public class PlayableNpcWizard : WizardWithCharacterPreview
    {
        public CharacterConfig characterConfig;
        public EquipItemSlotItemData[] equipItems;
        public MeshTypeFaceMeshObject[] faceMeshes;
        public Sprite portrait;
        public Texture2D texture;
        public Texture2D faceMeshTexture;

        private PlayableNpcConfig _selectedObject;


        [MenuItem("Tools/Character Editor/Create/Playable Npc Wizard...")]
        public static void CreateWizard()
        {
            DisplayWizard<PlayableNpcWizard>("Create Playable Npc", "Save new", "Update selected");
        }

        private void Awake()
        {
            Initialize();
            InitWizard();
        }

        private void OnSelectionChange()
        {
            var newTarget = Selection.activeObject as PlayableNpcConfig;
            if (newTarget != null && _selectedObject != newTarget)
            {
                InitWizard();
                OnValidate();
            }
        }

        private void OnValidate()
        {
            _updateCharacterPreview = true;
        }

        private void InitWizard()
        {
            var selectedObject = Selection.activeObject;
            if (selectedObject != null && selectedObject is PlayableNpcConfig npcConfig)
            {
                _selectedObject = npcConfig;
                InitValues(_selectedObject);
            }

            _updateCharacterPreview = true;
        }

        void OnWizardCreate()
        {
            var config = CreateInstance<PlayableNpcConfig>();

            SetValues(config);
            if (!Helper.TryCreateFolder(GetAssetDir(config.guid))) return;

            AssetDatabase.CreateAsset(config, GetAssetPath(config.guid));
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.FocusProjectWindow();
            Selection.activeObject = config;
        }

        void OnWizardOtherButton()
        {
            if (_selectedObject == null) return;

            SetValues(_selectedObject);

            EditorUtility.CopySerialized(_selectedObject, _selectedObject);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            _updateCharacterPreview = true;
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = _selectedObject;
        }


        protected override bool DrawWizardGUI()
        {
            var bodyOptions = new[]
            {
                GUILayout.Width(position.width - 30),
            };

            scrollPos = GUILayout.BeginScrollView(scrollPos, false, false, GUIStyle.none, GUI.skin.verticalScrollbar);

            GUILayout.BeginVertical(bodyOptions);
            DrawPortrait();

            base.DrawWizardGUI();
            GUILayout.EndVertical();

            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space();
            DrawCharacterPreview();

            return true;
        }

        private void DrawPortrait()
        {
            if (portrait == null) return;
            var croppedTexture = new Texture2D((int) portrait.rect.width, (int) portrait.rect.height);
            var pixels = portrait.texture.GetPixels((int) portrait.textureRect.x,
                (int) portrait.textureRect.y,
                (int) portrait.textureRect.width,
                (int) portrait.textureRect.height);
            croppedTexture.SetPixels(pixels);
            croppedTexture.Apply();
            GUILayout.Label(croppedTexture, GUILayout.Height(32f));
        }

        private void DrawCharacterPreview()
        {
            if (characterConfig == null) return;
            if (preview == null) preview = CreateInstance<Preview>();

            prect.height = position.height;
            prect.width = position.width;

            if (_updateCharacterPreview)
            {
                _updateCharacterPreview = false;

                GenerateCharacterPreview();
                preview.InitInstance(_currentCharacterPreview);
                preview.ResetPreviewFocus();

                Focus();
            }

            if (_currentCharacterPreview == null) return;
            preview.OnPreviewGUI(GUILayoutUtility.GetRect(100, position.height / 2.5f), GUIStyle.none);
        }

        private void GenerateCharacterPreview()
        {
            var configModel = AssetDatabase.LoadAssetAtPath<GameObject>(characterConfig.previewPrefabPath.path);
                _currentCharacterPreview = (GameObject) PrefabUtility.InstantiatePrefab(configModel);

            UpdateCharacterTexturesAndMeshes(_currentCharacterPreview);
        }

        protected override CharacterConfig GetSelectedCharacterConfig()
        {
            return characterConfig;
        }

        protected override EquipModelData[] GetSelectedCharacterMeshes()
        {
            var meshes = new List<EquipModelData>();
            foreach (var faceMesh in faceMeshes)
            {
                if (faceMesh.meshObject == null) continue;

                var mesh = new EquipModelData
                {
                    prefab = new PathData(AssetDatabase.GetAssetPath(faceMesh.meshObject)),
                    availableMeshes = new[] {faceMesh.meshType},
                    texture = new PathData(AssetDatabase.GetAssetPath(faceMeshTexture))
                };
                meshes.Add(mesh);
            }


            foreach (var item in equipItems)
            {
                if (item?.item == null) continue;

                foreach (var configItems in item.item.configsItems)
                {
                    if (configItems.configGuid != characterConfig.guid) continue;
                    meshes.AddRange(configItems.models);
                }
            }

            return meshes.ToArray();
        }

        protected override EquipTextureData[] GetSelectedCharacterTextures()
        {
            var textures = new List<EquipTextureData>();
            var skinTexture = new EquipTextureData
            {
                texture = new PathData(AssetDatabase.GetAssetPath(texture)),
                textureType = TextureType.Skin
            };
            textures.Add(skinTexture);

            foreach (var item in equipItems)
            {
                if (item?.item == null) continue;

                foreach (var configItems in item.item.configsItems)
                {
                    if (configItems.configGuid != characterConfig.guid) continue;
                    textures.AddRange(configItems.textures);
                }
            }

            return textures.ToArray();
        }


        void OnWizardUpdate()
        {
            helpString = "Enter npc details";
        }

        private void SetValues(PlayableNpcConfig config)
        {
            if (string.IsNullOrEmpty(config.guid))
                config.guid = Guid.NewGuid().ToString();

            config.portraitIconName = portrait.name;
            config.portraitIconPath = portrait.GetObjectPath();

            var texturePath = EditorHelper.SaveTexture(GetAssetDir(config.guid), texture);
            config.texturePath = new PathData(texturePath);

            if (faceMeshes.Length != 0)
            {
                texturePath = EditorHelper.SaveTexture(GetAssetDir(config.guid), faceMeshTexture);
                config.faceMeshTexturePath = new PathData(texturePath);
            }

            config.characterConfig = characterConfig;

            config.equipItems = new EquipItemSlotItem[equipItems.Length];
            for (var i = 0; i < equipItems.Length; i++)
                config.equipItems[i] = new EquipItemSlotItem(equipItems[i].itemSlot, equipItems[i].item);

            config.faceMeshs = new MeshTypeFaceMeshPath[faceMeshes.Length];
            for (var i = 0; i < faceMeshes.Length; i++)
                config.faceMeshs[i] = new MeshTypeFaceMeshPath(faceMeshes[i].meshType, AssetDatabase.GetAssetPath(faceMeshes[i].meshObject));
        }

        private void InitValues(PlayableNpcConfig config)
        {
            texture = AssetDatabase.LoadAssetAtPath<Texture2D>(config.texturePath.path);
            faceMeshTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(config.faceMeshTexturePath.path);
            var portraitSprites = AssetDatabase.LoadAllAssetsAtPath(config.portraitIconPath);
            portrait = portraitSprites.FirstOrDefault(sprite => sprite is Sprite && sprite.name == config.portraitIconName) as Sprite;
            
            characterConfig = config.characterConfig;

            equipItems = new EquipItemSlotItemData[config.equipItems.Length];
            for (var i = 0; i < config.equipItems.Length; i++)
            {
                var equipItem = config.equipItems[i];
                equipItems[i] = new EquipItemSlotItemData(equipItem.itemSlot, equipItem.item as EquipItemData);
            }

            faceMeshes = new MeshTypeFaceMeshObject[config.faceMeshs.Length];
            for (var i = 0; i < config.faceMeshs.Length; i++)
            {
                var faceMesh = config.faceMeshs[i];
                var obj = AssetDatabase.LoadAssetAtPath<GameObject>(faceMesh.meshPath.path);
                faceMeshes[i] = new MeshTypeFaceMeshObject(faceMesh.meshType, obj);
            }
        }

        private string GetAssetPath(string guid) =>
            $"{GetAssetDir(guid)}/{guid}.asset";

        private string GetAssetDir(string guid) =>
            $"Assets/Game/Data/PlayableNpc/{guid}";
    }
}
