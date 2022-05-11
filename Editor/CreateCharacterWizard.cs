using UnityEngine;
using System.Collections.Generic;
using CharacterEditor;
using UnityEditor;

namespace Editor
{
    public class CreateCharacterWizard : ScriptableWizard
    {
        public string folderName = "";
        public GameObject gameModel;
        public GameObject createGameModel;
        public GameObject previewModel;
        public GameObject enemyModel;
        public string headBone;

        public SkinnedMeshRenderer[] skinnedMeshes;
        public SkinnedMeshRenderer[] shortRobeMeshes;
        public SkinnedMeshRenderer[] longRobeMeshes;
        public SkinnedMeshRenderer[] cloakMeshes;

        [EnumFlag] public TextureType textureMask;
        private TextureType[] _availableTextures;

        public MeshTypeBone[] availableMeshes;

        private CharacterConfig _selectedObject;


        [MenuItem("Tools/Character Editor/Create Character Wizard...")]
        static void CreateWizard()
        {
            DisplayWizard<CreateCharacterWizard>("Create Character", "Save new", "Update selected");
        }

        void Awake()
        {
            var selectedObject = Selection.activeObject;
            if (selectedObject != null && selectedObject is CharacterConfig config)
            {
                _selectedObject = config;
                InitValues(_selectedObject);
                return;
            }

            InitDefaultValues();
        }

        void OnWizardCreate()
        {
            var config = ScriptableObject.CreateInstance<CharacterConfig>();

            SetValues(config);

            if (!System.IO.Directory.Exists(Application.dataPath + "/Packages/Character_Editor/Configs"))
            {
                AssetDatabase.CreateFolder(AssetsConstants.CharacterEditorRootPath, "Configs");
            }

            AssetDatabase.CreateAsset(config, GetAssetPath(folderName));
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

        private void InitDefaultValues()
        {
            availableMeshes = new MeshTypeBone[]
            {
                new MeshTypeBone(MeshType.Hair, "Bip01_Head"),
                new MeshTypeBone(MeshType.Helm, "Bip01_Head"),
                new MeshTypeBone(MeshType.Torso, "Bip01_Spine3"),
                new MeshTypeBone(MeshType.TorsoAdd, "Bip01_Spine3"),
                new MeshTypeBone(MeshType.LegLeft, "Bip01_L_Calf"),
                new MeshTypeBone(MeshType.LegRight, "Bip01_R_Calf"),
                new MeshTypeBone(MeshType.ShoulderLeft, "Bip01_L_UpperArm"),
                new MeshTypeBone(MeshType.ShoulderRight, "Bip01_R_UpperArm"),
                new MeshTypeBone(MeshType.ArmLeft, "Bip01_L_Forearm"),
                new MeshTypeBone(MeshType.ArmRight, "Bip01_R_Forearm"),
                new MeshTypeBone(MeshType.Belt, "Bip01_Pelvis"),
                new MeshTypeBone(MeshType.BeltAdd, "Bip01_Pelvis"),
                new MeshTypeBone(MeshType.HandLeft, "Bip01_L_Weapon"),
                new MeshTypeBone(MeshType.HandRight, "Bip01_R_Weapon"),
                new MeshTypeBone(MeshType.Beard, "Bip01_Jaw"),
                new MeshTypeBone(MeshType.FaceFeature, "Bip01_Jaw"),
            };

            headBone = "Bip01_Head";
        }

        private void SetValues(CharacterConfig config)
        {
            InitTextureTypes();

            config.prefabPath = GetPrefabPath(gameModel);
            config.createGamePrefabPath = GetPrefabPath(createGameModel);
            config.previewPrefabPath = GetPrefabPath(previewModel);
            config.enemyPrefabPath = GetPrefabPath(enemyModel);


            config.folderName = folderName;
            config.availableMeshes = availableMeshes;
            config.availableTextures = _availableTextures;
            config.headBone = headBone;

            config.skinnedMeshes = new string[skinnedMeshes.Length];
            for (var i = 0; i < skinnedMeshes.Length; i++)
                config.skinnedMeshes[i] = skinnedMeshes[i].name;

            config.cloakMeshes = new string[cloakMeshes.Length];
            for (var i = 0; i < cloakMeshes.Length; i++)
                config.cloakMeshes[i] = cloakMeshes[i].name;

            config.shortRobeMeshes = new string[shortRobeMeshes.Length];
            for (var i = 0; i < shortRobeMeshes.Length; i++)
                config.shortRobeMeshes[i] = shortRobeMeshes[i].name;

            config.longRobeMeshes = new string[longRobeMeshes.Length];
            for (var i = 0; i < longRobeMeshes.Length; i++)
                config.longRobeMeshes[i] = longRobeMeshes[i].name;

            if (string.IsNullOrEmpty(config.guid))
                config.guid = System.Guid.NewGuid().ToString();
        }

        private string GetPrefabPath(GameObject asset)
        {
            if (asset == null) return null;

            var prefabPath = AssetDatabase.GetAssetPath(asset);
            if (!string.IsNullOrEmpty(prefabPath)) return prefabPath;

            var prefab = PrefabUtility.GetCorrespondingObjectFromSource(asset);
            return AssetDatabase.GetAssetPath(prefab);
        }

        private void InitValues(CharacterConfig config)
        {
            folderName = config.folderName;
            gameModel = AssetDatabase.LoadAssetAtPath<GameObject>(config.prefabPath);
            createGameModel = AssetDatabase.LoadAssetAtPath<GameObject>(config.createGamePrefabPath);
            previewModel = AssetDatabase.LoadAssetAtPath<GameObject>(config.previewPrefabPath);
            headBone = config.headBone;
            availableMeshes = config.availableMeshes;
            _availableTextures = config.availableTextures;

            InitSkinnedMeshes(config.skinnedMeshes, ref skinnedMeshes);
            InitSkinnedMeshes(config.cloakMeshes, ref cloakMeshes);
            InitSkinnedMeshes(config.shortRobeMeshes, ref shortRobeMeshes);
            InitSkinnedMeshes(config.longRobeMeshes, ref longRobeMeshes);

            InitTextureMask();
        }

        private void InitSkinnedMeshes(string[] meshes, ref SkinnedMeshRenderer[] renderers)
        {
            if (meshes == null) return;

            renderers = new SkinnedMeshRenderer[meshes.Length];
            for (var i = 0; i < meshes.Length; i++)
            {
                var transform = gameModel.transform.FindTransform(meshes[i]);
                renderers[i] = transform.GetComponent<SkinnedMeshRenderer>();
            }
        }

        private void InitTextureTypes()
        {
            var list = new List<TextureType>();
            foreach (var enumValue in System.Enum.GetValues(typeof(TextureType)))
            {
                var checkBit = (int) textureMask & (int) enumValue;
                if (checkBit != 0)
                    list.Add((TextureType) enumValue);
            }

            _availableTextures = list.ToArray();
        }

        private void InitTextureMask()
        {
            textureMask = 0;
            foreach (var t in _availableTextures)
                textureMask |= t;
        }


        private string GetAssetPath(string assetName) =>
            $"{AssetsConstants.CharacterStaticDataPath}/CharacterConfigs/{assetName}.asset";
    }
}