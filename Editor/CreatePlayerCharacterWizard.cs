using System;
using System.Collections.Generic;
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

    public class CreatePlayerCharacterWizard : WizardWithCharacterPreview
    {
        public CharacterConfig characterConfig;
        public EquipItemSlotItemData[] equipItems;
        public MeshTypeFaceMeshObject[] faceMeshs;
        public Sprite portrait;
        public Texture2D texture;
        public Texture2D faceMeshTexture;

        private PlayableNpcConfig _selectedObject;


        [MenuItem("Tools/Character Editor/Create Player Character Wizard...")]
        static void CreateWizard()
        {
            ScriptableWizard.DisplayWizard<CreatePlayerCharacterWizard>("Create Player Character", "Save new",
                "Update selected");
        }

        void Awake()
        {
            var selectedObject = Selection.activeObject;
            if (selectedObject != null && selectedObject is PlayableNpcConfig)
            {
                _selectedObject = selectedObject as PlayableNpcConfig;
                InitValues(_selectedObject);
            }
        }

        void OnWizardCreate()
        {
            var config = ScriptableObject.CreateInstance<PlayableNpcConfig>();

            SetValues(config);

            if (!System.IO.Directory.Exists(Application.dataPath + "/Game/Data"))
                AssetDatabase.CreateFolder("Assets/Game", "Data");

            if (!System.IO.Directory.Exists(Application.dataPath + "/Game/Data/PlayerCharacters"))
                AssetDatabase.CreateFolder("Assets/Game/Data", "PlayerCharacters");

            AssetDatabase.CreateAsset(config, GetAssetPath(config.guid));
            AssetDatabase.SaveAssets();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = config;
            _updateCharacterPreview = true;
        }

        void OnWizardOtherButton()
        {
            if (_selectedObject == null)
                return;

            SetValues(_selectedObject);

            EditorUtility.CopySerialized(_selectedObject, _selectedObject);
            AssetDatabase.SaveAssets();

            EditorUtility.FocusProjectWindow();
            Selection.activeObject = _selectedObject;
        }

        void OnValidate()
        {
            _updateCharacterPreview = true;
        }


        protected override bool DrawWizardGUI()
        {
            var bodyOptions = new[]
            {
                GUILayout.Width(position.width - 30),
            };

            scrollPos = GUILayout.BeginScrollView(scrollPos, false, false, GUIStyle.none, GUI.skin.verticalScrollbar);

            GUILayout.BeginVertical(bodyOptions);
            base.DrawWizardGUI();
            GUILayout.EndVertical();

            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space();
            DrawCharacterPreview();

            return true;
        }

        private void DrawCharacterPreview()
        {
            if (p == null) p = CreateInstance<Preview>();

            prect.height = position.height;
            prect.width = position.width;



            if (_updateCharacterPreview)
            {
                _updateCharacterPreview = false;

                GenerateCharacterPreview();
                p.InitInstance(_currentCharacterPreview);
                p.ResetPreviewFocus();

                //update for preview
                if (_currentCharacterPreview == null) return;
                _currentCharacterPreview.SetActive(false);
                _currentCharacterPreview.SetActive(true);

                Focus();
            }

            if (_currentCharacterPreview == null) return;
            p.OnPreviewGUI(GUILayoutUtility.GetRect(100, position.height / 2.5f), GUIStyle.none);
        }

        protected override void GenerateCharacterPreview()
        {
            if (characterConfig == null) return;

      
                var configModel = AssetDatabase.LoadAssetAtPath<GameObject>(characterConfig.previewPrefabPath);
                _currentCharacterPreview = (GameObject) PrefabUtility.InstantiatePrefab(configModel);
            


            UpdateCharacterTexturesAndMeshes();
        }

        protected override CharacterConfig GetSelectedCharacterConfig()
        {
            return characterConfig;
        }

        protected override EquipModelData[] GetSelectedCharacterMeshes()
        {
            var meshes = new List<EquipModelData>();
            foreach (var faceMesh in faceMeshs)
            {
                var mesh = new EquipModelData();
                mesh.prefabPath = AssetDatabase.GetAssetPath(faceMesh.meshObject);
                mesh.availableMeshes = new MeshType[] { faceMesh.meshType};
                mesh.texturePath = AssetDatabase.GetAssetPath(faceMeshTexture);
                meshes.Add(mesh);
            }


            foreach (var item in equipItems)
            {
                foreach (var configItems in item.item.configsItems)
                {
                    meshes.AddRange(configItems.models);
                }
            }

            return meshes.ToArray();
        }

        protected override EquipTextureData[] GetSelectedCharacterTextures()
        {
            var textures = new List<EquipTextureData>();
            var skinTexture = new EquipTextureData();
            skinTexture.texturePath = AssetDatabase.GetAssetPath(texture);
            skinTexture.textureType = TextureType.Skin;
            textures.Add(skinTexture);

            foreach (var item in equipItems)
            {
                foreach (var configItems in item.item.configsItems)
                {
                    textures.AddRange(configItems.textures);

                }
            }

            return textures.ToArray();
        }


        void OnWizardUpdate()
        {
            helpString = "Enter character details";
        }

        private void SetValues(PlayableNpcConfig config)
        {
            config.portraitIconName = portrait.name;
            config.portraitIconPath = AssetDatabase.GetAssetPath(portrait);
            if (config.portraitIconPath == "")
            {
                var prefab = PrefabUtility.GetCorrespondingObjectFromSource(portrait);
                config.portraitIconPath = AssetDatabase.GetAssetPath(prefab);
            }

            config.texturePath = AssetDatabase.GetAssetPath(texture);
            if (config.texturePath == "")
            {
                var prefab = PrefabUtility.GetCorrespondingObjectFromSource(texture);
                config.texturePath = AssetDatabase.GetAssetPath(prefab);
            }

            config.faceMeshTexturePath = AssetDatabase.GetAssetPath(faceMeshTexture);
            if (config.faceMeshTexturePath == "")
            {
                var prefab = PrefabUtility.GetCorrespondingObjectFromSource(faceMeshTexture);
                config.faceMeshTexturePath = AssetDatabase.GetAssetPath(prefab);
            }

            config.characterConfig = characterConfig;

            config.equipItems = new EquipItemSlotItem[equipItems.Length];
            for (var i = 0; i < equipItems.Length; i++)
            {
                config.equipItems[i] = new EquipItemSlotItem(equipItems[i].itemSlot, equipItems[i].item);
            }

            config.faceMeshs = new MeshTypeFaceMeshPath[faceMeshs.Length];
            for (var i = 0; i < faceMeshs.Length; i++)
            {
                config.faceMeshs[i] = new MeshTypeFaceMeshPath(faceMeshs[i].meshType, AssetDatabase.GetAssetPath(faceMeshs[i].meshObject));
            }

            if (string.IsNullOrEmpty(config.guid))
                config.guid = System.Guid.NewGuid().ToString();
        }

        private void InitValues(PlayableNpcConfig config)
        {
            texture = AssetDatabase.LoadAssetAtPath<Texture2D>(config.texturePath);
            faceMeshTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(config.faceMeshTexturePath);
            var portraitSprites = AssetDatabase.LoadAllAssetsAtPath(config.portraitIconPath);
            if (portraitSprites != null && portraitSprites.Length > 0)
            {
                foreach (var go in portraitSprites)
                {
                    var sprite = go as Sprite;
                    if (sprite != null && sprite.name == config.portraitIconName)
                    {
                        portrait = sprite;
                        break;
                    }
                }
            }
            
            characterConfig = config.characterConfig;

            equipItems = new EquipItemSlotItemData[config.equipItems.Length];
            for (var i = 0; i < config.equipItems.Length; i++)
            {
                var equipItem = config.equipItems[i];
                equipItems[i] = new EquipItemSlotItemData(equipItem.itemSlot, equipItem.item as EquipItemData);
            }

            faceMeshs = new MeshTypeFaceMeshObject[config.faceMeshs.Length];
            for (var i = 0; i < config.faceMeshs.Length; i++)
            {
                var faceMesh = config.faceMeshs[i];
                var obj = AssetDatabase.LoadAssetAtPath<GameObject>(faceMesh.meshPath);
                faceMeshs[i] = new MeshTypeFaceMeshObject(faceMesh.meshType, obj);
            }
        }

        private string GetAssetPath(string guid) =>
            $"Assets/Game/Data/PlayerCharacters/{guid}.asset";
    }
}
