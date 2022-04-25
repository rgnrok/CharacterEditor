using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CharacterEditor
{
    public class CreateEnemyWizard : ScriptableWizard
    {
        public CharacterConfig entityConfig;
        public PrefabBoneData prefabBoneData;
        public Texture2D skinTexture;
        public Texture2D faceTexture;
        public Texture2D armorTexture;
        public Material material;
        public Sprite portrait;

        private EnemyConfig _selectedObject;

        [MenuItem("Tools/Character Editor/Create Enemy Wizard...")]
        static void CreateWizard()
        {
            ScriptableWizard.DisplayWizard<CreateEnemyWizard>("Create Enemy", "Save new",
                "Update selected");
        }

        void Awake()
        {
            var selectedObject = Selection.activeObject;
            if (selectedObject != null && selectedObject is EnemyConfig)
            {
                _selectedObject = selectedObject as EnemyConfig;
                InitValues(_selectedObject);
            }
        }

        void OnWizardCreate()
        {
            var config = ScriptableObject.CreateInstance<EnemyConfig>();

            SetValues(config);

            if (!System.IO.Directory.Exists(Application.dataPath + "/Game/Data"))
                AssetDatabase.CreateFolder("Assets/Game", "Data");

            if (!System.IO.Directory.Exists(Application.dataPath + "/Game/Data/Enemies"))
                AssetDatabase.CreateFolder("Assets/Game/Data", "Enemies");


            AssetDatabase.CreateAsset(config, GetAssetPath(config.guid));
            AssetDatabase.SaveAssets();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = config;
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


        void OnWizardUpdate()
        {
            helpString = "Enter character details";
        }

        private void SetValues(EnemyConfig config)
        {
            config.prefabBoneData = prefabBoneData;
            config.entityConfig = entityConfig;

            config.portraitIconName = portrait.name;
            config.portraitIconPath = AssetDatabase.GetAssetPath(portrait);
            if (config.portraitIconPath == "")
            {
                var prefab = PrefabUtility.GetCorrespondingObjectFromSource(portrait);
                config.portraitIconPath = AssetDatabase.GetAssetPath(prefab);
            }

            config.texturePath = AssetDatabase.GetAssetPath(skinTexture);
            if (config.texturePath == "")
            {
                var prefab = PrefabUtility.GetCorrespondingObjectFromSource(skinTexture);
                config.texturePath = AssetDatabase.GetAssetPath(prefab);
            }

            config.faceMeshTexturePath = AssetDatabase.GetAssetPath(faceTexture);
            if (config.faceMeshTexturePath == "")
            {
                var prefab = PrefabUtility.GetCorrespondingObjectFromSource(faceTexture);
                config.faceMeshTexturePath = AssetDatabase.GetAssetPath(prefab);
            }

            config.armorTexturePath = AssetDatabase.GetAssetPath(armorTexture);
            if (config.armorTexturePath == "")
            {
                var prefab = PrefabUtility.GetCorrespondingObjectFromSource(armorTexture);
                config.armorTexturePath = AssetDatabase.GetAssetPath(prefab);
            }

            config.materialPath = AssetDatabase.GetAssetPath(material);
            if (config.materialPath == "")
            {
                var prefab = PrefabUtility.GetCorrespondingObjectFromSource(material);
                config.materialPath = AssetDatabase.GetAssetPath(prefab);
            }

            if (string.IsNullOrEmpty(config.guid))
                config.guid = System.Guid.NewGuid().ToString();
        }

        private void InitValues(EnemyConfig config)
        {
            prefabBoneData = config.prefabBoneData;
            entityConfig = config.entityConfig;
            skinTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(config.texturePath);
            faceTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(config.faceMeshTexturePath);
            armorTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(config.armorTexturePath);
            material = AssetDatabase.LoadAssetAtPath<Material>(config.materialPath);

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
        }

        private string GetAssetPath(string guid) => 
            "Assets/Game/Data/Enemies/" + guid + ".asset";
    }
}