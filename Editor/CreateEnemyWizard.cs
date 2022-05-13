using System.Collections;
using System.Collections.Generic;
using CharacterEditor;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    public class CreateEnemyWizard : ScriptableWizard
    {
        public GameObject prefab;
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
            config.prefabPath = new PathData(prefab.GetObjectPath());

            config.portraitIconName = portrait.name;
            config.portraitIconPath = portrait.GetObjectPath();

            config.texturePath = new PathData(skinTexture.GetObjectPath());
            config.faceMeshTexturePath = new PathData(faceTexture.GetObjectPath());
            config.armorTexturePath = new PathData(armorTexture.GetObjectPath());
            config.materialPath = new PathData(material.GetObjectPath());
       

            if (string.IsNullOrEmpty(config.guid))
                config.guid = System.Guid.NewGuid().ToString();
        }

        private void InitValues(EnemyConfig config)
        {
            prefabBoneData = config.prefabBoneData;
            prefab = AssetDatabase.LoadAssetAtPath<GameObject>(config.prefabPath.path);
            skinTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(config.texturePath.path);
            faceTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(config.faceMeshTexturePath.path);
            armorTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(config.armorTexturePath.path);
            material = AssetDatabase.LoadAssetAtPath<Material>(config.materialPath.path);

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