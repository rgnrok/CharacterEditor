using CharacterEditor;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    public class EnemyWizard : ScriptableWizard
    {
        public GameObject prefab;
        public string[] skinnedMeshes;
        public PrefabBoneData prefabBoneData;
        public Texture2D skinTexture;
        public Texture2D faceTexture;
        public Texture2D armorTexture;
        public Material material;
        public Sprite portrait;

        private EnemyConfig _selectedObject;

        [MenuItem("Tools/Character Editor/Create/Enemy Wizard...")]
        public static void CreateWizard()
        {
            DisplayWizard<EnemyWizard>("Create Enemy", "Save new",
                "Update selected");
        }

        void Awake()
        {
            var selectedObject = Selection.activeObject;
            if (selectedObject != null && selectedObject is EnemyConfig enemyConfig)
            {
                _selectedObject = enemyConfig;
                InitValues(_selectedObject);
            }
        }

        void OnWizardCreate()
        {
            var config = CreateInstance<EnemyConfig>();

            SetValues(config);

            if (!Helper.TryCreateFolder("Assets/Game/Data/Enemies")) return;

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
            if (string.IsNullOrEmpty(config.guid))
                config.guid = System.Guid.NewGuid().ToString();

            config.prefabPath = new PathData(prefab.GetObjectPath());

            config.portraitIconName = portrait.name;
            config.portraitIconPath = portrait.GetObjectPath();

            config.skinnedMeshes = skinnedMeshes;

            var path = EditorHelper.SaveTexture(GetAssetDir(config.guid), skinTexture);
            config.texturePath = new PathData(path);

            path = EditorHelper.SaveTexture(GetAssetDir(config.guid), faceTexture);
            config.faceMeshTexturePath = new PathData(path);

            path = EditorHelper.SaveTexture(GetAssetDir(config.guid), armorTexture);
            config.armorTexturePath = new PathData(path);

            path = EditorHelper.SaveAsset(GetAssetDir(config.guid), $"{material.name}.mat", material);
            config.materialPath = new PathData(path);
            
            var prefabBonePath = EditorHelper.SaveAsset(GetAssetDir(config.guid), $"{prefabBoneData.name}.asset", prefabBoneData);
            config.prefabBoneData = AssetDatabase.LoadAssetAtPath<PrefabBoneData>(prefabBonePath);
        }

        private void InitValues(EnemyConfig config)
        {
            prefabBoneData = config.prefabBoneData;
            skinnedMeshes = config.skinnedMeshes;
            prefab = AssetDatabase.LoadAssetAtPath<GameObject>(config.prefabPath.path);
            skinTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(config.texturePath.path);
            faceTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(config.faceMeshTexturePath.path);
            armorTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(config.armorTexturePath.path);
            material = AssetDatabase.LoadAssetAtPath<Material>(config.materialPath.path);

            var portraitSprites = AssetDatabase.LoadAllAssetsAtPath(config.portraitIconPath);
            portrait = portraitSprites.FirstOrDefault(sprite => sprite is Sprite && sprite.name == config.portraitIconName) as Sprite;
        }

        private string GetAssetPath(string guid) =>
            $"{GetAssetDir(guid)}/{guid}.asset";

        private string GetAssetDir(string guid) =>
            $"Assets/Game/Data/Enemies/{guid}";
    }
}