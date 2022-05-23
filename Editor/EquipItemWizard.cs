using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CharacterEditor;
using CharacterEditor.CharacterInventory;
using CharacterEditor.Services;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    public class EquipItemWizard : ScriptableWizard
    {
        [Serializable]
        public class ConfigEquipItemInfo
        {
            public CharacterConfig characterConfig;
            public EquipItemMeshesInfo[] models;
            public EquipItemTexturesInfo[] textures;
        }

        [Serializable]
        public class EquipItemTexturesInfo
        {
            public TextureType textureType;
            public Texture2D texture;
        }

        [Serializable]
        public class EquipItemMeshesInfo
        {
            public GameObject model;
            public GameObject additionalModel;
            public Texture2D texture;
            public Texture2D additionalTexture;
            [EnumFlag] public MeshType meshMask;
            [HideInInspector]
            public MeshType[] availableMeshes;
        }

        [Header("General")]
        public Sprite icon;
        public GameObject prefab;
        public string itemName;

        [Header("Character configs")]
        public ConfigEquipItemInfo[] configInfo;

        private EquipItemType itemType;
        private EquipItemSubType itemSubType;
        private EquipItemPantsSubType pantsSubType;
        private EquipItemWeaponSubType weaponSubType;
        
        [EnumFlag] public MeshType hidedMeshMask;
        private MeshType[] hidedMeshes;

        Preview p;

        private bool _isRandomStats;
        private int _randomStatsCount;

        private EquipItemData _selectedObject;
        private SerializedObject _serializedObject;

        private UnityEditor.Editor _gameObjectEditor;

        private int _selectedMeshIndex;
        private List<string> _mesheTitles = new List<string>();
        private List<GameObject> _meshes = new List<GameObject>();

        private int _selectedConfigIndex;
        private List<string> _configTitles = new List<string>();
        private List<CharacterConfig> _configs = new List<CharacterConfig>();

        private GameObject _currentCharacterPreview;
        private Dictionary<int, GameObject> _characterPreviews = new Dictionary<int, GameObject>();
        private Dictionary<string, GameObject> _itemModelPreviews = new Dictionary<string, GameObject>();

        private Vector2 scrollPos;
        private bool _updateCharacterPreview;
        private int _currentModelAndTextureHash;
        private int _prevModelAndTextureHash;
        private Rect prect = new Rect();
        private MergeTextureService _mergeTextureService;

        [MenuItem("Tools/Character Editor/Items/Equip Item Wizard")]
        public static void CreateWizard()
        {
            DisplayWizard<EquipItemWizard>("Create item", "Save new", "Update selected");
        }

        void OnSelectionChange()
        {
            var newTarget = Selection.activeObject as EquipItemData;
            if (newTarget != null && _selectedObject != newTarget)
            {
                InitWizard();
                OnValidate();
            }
        }
        
        void Awake()
        {
            InitWizard();
        }

        private void InitWizard()
        {
            _mergeTextureService = new MergeTextureService();
            _selectedObject = Selection.activeObject as EquipItemData;
            if (_selectedObject != null)
            {
                InitValues();
            }
            else
            {
                _selectedObject = CreateInstance<EquipItemData>();
                configInfo = new ConfigEquipItemInfo[0];
            }

            _serializedObject = new SerializedObject(_selectedObject);

            UpdateMeshTitles();
        }

        void OnDestroy()
        {
            foreach (var characterPreview in _characterPreviews.Values)
            {
                foreach (var render in characterPreview.GetComponentsInChildren<SkinnedMeshRenderer>())
                {
                    render.sharedMaterial.mainTexture = null;
                }

                if (characterPreview != null) GameObject.DestroyImmediate(characterPreview);
            }
        }


        void OnValidate()
        {
            if (_serializedObject == null) _serializedObject = new SerializedObject(_selectedObject);
            UpdateMeshTitles();
            _currentModelAndTextureHash = CalculatePreviewHash();
            if (_currentModelAndTextureHash != _prevModelAndTextureHash)
            {
                _updateCharacterPreview = true;
            }
            _prevModelAndTextureHash = _currentModelAndTextureHash;
        }

        private void UpdateMeshTitles()
        {
            _mesheTitles.Clear();
            _meshes.Clear();

            _configTitles.Clear();
            _configs.Clear();
            foreach (var config in configInfo)
            {
                if (config.characterConfig == null) continue;

                _configTitles.Add(config.characterConfig.folderName);
                _configs.Add(config.characterConfig);

                foreach (var modelInfo in config.models)
                {
                    if (modelInfo.model == null) continue;
                    
                    _meshes.Add(modelInfo.model);
                    _mesheTitles.Add(modelInfo.model.name);

                    if (modelInfo.additionalModel != null)
                    {
                        _meshes.Add(modelInfo.additionalModel);
                        _mesheTitles.Add(modelInfo.additionalModel.name);
                    }
                }
            }
        }

        protected int CalculatePreviewHash()
        {
            if (_configs.Count == 0) return 0;

            var strBuilder = new StringBuilder();
            var selectedConfig = _configs[_selectedConfigIndex];
            foreach (var configData in configInfo)
            {
                if (configData.characterConfig.guid != selectedConfig.guid) continue;

                foreach (var model in configData.models)
                {
                    if (model.model != null) strBuilder.Append(model.model.name);
                    if (model.texture != null) strBuilder.Append(model.texture.name);
                    strBuilder.Append(model.meshMask);
                }

                foreach (var texture in configData.textures)
                {
                    if (texture.texture != null) strBuilder.Append(texture.texture.name);
                    strBuilder.Append(texture.textureType);
                }

                return strBuilder.ToString().GetHashCode();
            }

            return 0;
        }


        protected override bool DrawWizardGUI()
        {
            var bodyOptions = new[]
            {
                GUILayout.Width(position.width - 30),
            };

            scrollPos = GUILayout.BeginScrollView(scrollPos, false, false, GUIStyle.none, GUI.skin.verticalScrollbar);

            if (icon != null) GUILayout.Label(icon.texture, GUILayout.Height(32f));

            GUILayout.BeginVertical(bodyOptions);
            base.DrawWizardGUI();
            GUILayout.EndVertical();

            EditorGUILayout.Space();
            DrawItemTypeAndSubType();

            EditorGUILayout.Space();
            DrawItemStats();
            EditorGUILayout.EndScrollView();
        

            EditorGUILayout.Space();
            //DrawItemPreview();
            DrawCharacterPreview();


            return true;
        }


        private void DrawItemTypeAndSubType()
        {
            EditorGUILayout.LabelField("Item type", EditorStyles.boldLabel);
            var options = new []
            {
                GUILayout.Width(position.width - 8),
                GUILayout.Height(20),
            };

            itemType = (EquipItemType)EditorGUI.EnumPopup(GUILayoutUtility.GetRect(titleContent, GUI.skin.label, options), "Type", itemType);

            switch (itemType)
            {
                case EquipItemType.Pants:
                    pantsSubType = (EquipItemPantsSubType)EditorGUI.EnumPopup(
                        GUILayoutUtility.GetRect(titleContent, GUI.skin.label, options), "Sub Type", itemSubType);
                    itemSubType = (EquipItemSubType)pantsSubType;
                    break;
                case EquipItemType.Weapon:
                    weaponSubType = (EquipItemWeaponSubType)EditorGUI.EnumPopup(
                        GUILayoutUtility.GetRect(titleContent, GUI.skin.label, options), "Sub Type", itemSubType);
                    itemSubType = (EquipItemSubType) weaponSubType;
                    break;
            }
        }

        private void DrawItemStats()
        {
            EditorGUILayout.LabelField("Stats and Modifiers", EditorStyles.boldLabel);
            var options = new[]
            {
                GUILayout.Width(position.width - 8),
                GUILayout.Height(20),
            };

            _isRandomStats = EditorGUI.Toggle(GUILayoutUtility.GetRect(titleContent, GUI.skin.label, options),
                "Generate random stats",
                _isRandomStats);

            if (_isRandomStats)
            {
                _randomStatsCount = EditorGUI.IntField(GUILayoutUtility.GetRect(titleContent, GUI.skin.label, options),
                    "Random stats count", _randomStatsCount);
                return;
            }


            EditorGUILayout.PropertyField(_serializedObject.FindProperty("stats"), true);

        }

        private void DrawItemPreview()
        {
            _selectedMeshIndex = EditorGUI.Popup(
                GUILayoutUtility.GetRect(this.titleContent, GUI.skin.label, new []
                {
                    GUILayout.Width(position.width - 8),
                    GUILayout.Height(20),
                }),
                "Preview",
                _selectedMeshIndex,
                _mesheTitles.ToArray()
            );

            if (_mesheTitles.Count != 0)
            {
                var uvModelPath = AssetDatabase.GetAssetPath(_meshes[_selectedMeshIndex]);
                uvModelPath = uvModelPath.Replace("/StaticModel/", "/Model/");

                var uvModel = AssetDatabase.LoadAssetAtPath<GameObject>(uvModelPath);
                UnityEditor.Editor.CreateCachedEditor(uvModel, null, ref _gameObjectEditor);

                _gameObjectEditor.OnPreviewGUI(GUILayoutUtility.GetRect(200, 200), GUIStyle.none);
            }
        }



        private void DrawCharacterPreview()
        {
            if (p == null) p = CreateInstance<Preview>();

            prect.height = position.height;
            prect.width = position.width;

            var index = EditorGUI.Popup(
                GUILayoutUtility.GetRect(this.titleContent, GUI.skin.label, new[]
                {
                    GUILayout.Width(position.width - 8),
                    GUILayout.Height(20),
                }),
                "Preview Character",
                _selectedConfigIndex,
                _configTitles.ToArray()
            );

            if (_updateCharacterPreview || _selectedConfigIndex != index || (_currentCharacterPreview == null && _configTitles.Count > 0))
            {
                _updateCharacterPreview = false;

                GenerateCharacterPreview(index);
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

        private void GenerateCharacterPreview(int configIndex)
        {
            if (_configTitles.Count == 0) return;

            if (_currentCharacterPreview != null) _currentCharacterPreview.SetActive(false);
            _selectedConfigIndex = configIndex;

            if (_characterPreviews.ContainsKey(_selectedConfigIndex))
            {
                _currentCharacterPreview = _characterPreviews[_selectedConfigIndex];
                _currentCharacterPreview.SetActive(true);
            }
            else
            {
                var selectedConfig = _configs[_selectedConfigIndex];
                var configModel = AssetDatabase.LoadAssetAtPath<GameObject>(selectedConfig.previewPrefabPath.path);
                _currentCharacterPreview = (GameObject)PrefabUtility.InstantiatePrefab(configModel);
                _characterPreviews[_selectedConfigIndex] = _currentCharacterPreview;
            }

            UpdateCharacterTexturesAndMeshes();
        }

        private void UpdateCharacterTexturesAndMeshes()
        {
            if (_currentCharacterPreview == null) return;
            if (_configs.Count <= _selectedConfigIndex) return;

            foreach (var itemModelObj in _itemModelPreviews.Values)
            {
                itemModelObj.SetActive(false);
            }
            UpdateCharacterMeshes();
            UpdateCharacterTextures();
        }

        private void UpdateCharacterMeshes()
        {
            var selectedConfig = _configs[_selectedConfigIndex];
            foreach (var configData in configInfo)
            {
                if (configData.characterConfig.guid != selectedConfig.guid) continue;

                foreach (var model in configData.models)
                {
                    var availableMeshes = GetAvailableMeshes(model.meshMask);
                    if (availableMeshes.Length == 0) continue;

                    MeshTypeBone meshBone = null;
                    foreach (var avMesh in selectedConfig.availableMeshes)
                    {
                        if (avMesh.mesh != availableMeshes[0]) continue;
                        meshBone = avMesh;
                        break;
                    }
                    if (meshBone == null) continue;

                    var uvModelPath = AssetDatabase.GetAssetPath(model.model);
                    uvModelPath = uvModelPath.Replace("/StaticModel/", "/Model/");

                    GameObject uvPrefab;
                    if (_itemModelPreviews.ContainsKey(uvModelPath))
                    {
                        uvPrefab = _itemModelPreviews[uvModelPath];
                        uvPrefab.SetActive(true);
                    }
                    else
                    {
                        var uvModel = AssetDatabase.LoadAssetAtPath<GameObject>(uvModelPath);
                        uvPrefab = (GameObject)PrefabUtility.InstantiatePrefab(uvModel);
                        _itemModelPreviews[uvModelPath] = uvPrefab;
                    }

                    foreach (var meshRender in uvPrefab.GetComponentsInChildren<MeshRenderer>())
                    {
                        meshRender.sharedMaterial.mainTexture = model.texture;
                    }

                    var bone = Helper.FindTransform(_currentCharacterPreview.transform, meshBone.boneName);
                    if (bone != null)
                    {
                        uvPrefab.transform.parent = bone;
                        uvPrefab.transform.localPosition = Vector3.zero;
                        uvPrefab.transform.localRotation = Quaternion.identity;
                    }
                }
                
                break;
            }
        }

        private void UpdateCharacterTextures()
        {
            var selectedConfig = _configs[_selectedConfigIndex];

            var mat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Editor/Materials/SkinAndClothRenderMaterial.mat");
            var renderClothTexture = new RenderTexture(Constants.SKIN_TEXTURE_ATLAS_SIZE, Constants.SKIN_TEXTURE_ATLAS_SIZE, 1, RenderTextureFormat.ARGB32);

            foreach (var configData in configInfo)
            {
                if (configData.characterConfig.guid != selectedConfig.guid) continue;

                var configTextures =
                    configData.textures
                        .Where(t => Helper.GetShaderTextureName(t.textureType) != null)
                        .ToDictionary(t => Helper.GetShaderTextureName(t.textureType), t => t.texture);

                _mergeTextureService.MergeTextures(mat, renderClothTexture, configTextures);
                
                RenderTexture.active = renderClothTexture;
                var characterTexture = new Texture2D(Constants.SKIN_TEXTURE_ATLAS_SIZE, Constants.SKIN_TEXTURE_ATLAS_SIZE, TextureFormat.RGB24, false);
                characterTexture.ReadPixels(new Rect(0, 0, renderClothTexture.width, renderClothTexture.height), 0, 0);
                characterTexture.Apply();

                foreach (var render in _currentCharacterPreview.GetComponentsInChildren<SkinnedMeshRenderer>())
                    render.sharedMaterial.mainTexture = characterTexture;

                break;
            }
        }

        void InitValues()
        {
            itemName = _selectedObject.itemName;
            itemType = _selectedObject.itemType;
            itemSubType = _selectedObject.itemSubType;

            if (_selectedObject.configsItems == null) return;

            if (!string.IsNullOrEmpty(_selectedObject.icon.path))
                icon = AssetDatabase.LoadAssetAtPath<Sprite>(_selectedObject.icon.path);

            if (!string.IsNullOrEmpty(_selectedObject.prefab.path))
                prefab = AssetDatabase.LoadAssetAtPath<GameObject>(_selectedObject.prefab.path);

            configInfo = new ConfigEquipItemInfo[_selectedObject.configsItems.Length];
            for (var i = 0; i < _selectedObject.configsItems.Length; i++)
            {
                var configItemInfoData = _selectedObject.configsItems[i];
                configInfo[i] = new ConfigEquipItemInfo();
                configInfo[i].characterConfig = configItemInfoData.characterConfig;

                if (configItemInfoData.textures != null)
                {
                    configInfo[i].textures = new EquipItemTexturesInfo[configItemInfoData.textures.Length];
                    for (int j = 0; j < configItemInfoData.textures.Length; j++)
                    {
                        var textureData = configItemInfoData.textures[j];
                        configInfo[i].textures[j] = new EquipItemTexturesInfo();
                        configInfo[i].textures[j].textureType = textureData.textureType;
                        configInfo[i].textures[j].texture = AssetDatabase.LoadAssetAtPath<Texture2D>(textureData.texturePath);
                    }
                }

                if (configItemInfoData.models != null)
                {
                    configInfo[i].models = new EquipItemMeshesInfo[configItemInfoData.models.Length];
                    for (int k = 0; k < configItemInfoData.models.Length; k++)
                    {
                        var modelData = configItemInfoData.models[k];
                        configInfo[i].models[k] = new EquipItemMeshesInfo();
                        configInfo[i].models[k].texture = AssetDatabase.LoadAssetAtPath<Texture2D>(modelData.texturePath);
                        if (modelData.additionalTexturePath != null && !modelData.additionalTexturePath.Equals(""))
                            configInfo[i].models[k].additionalTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(modelData.additionalTexturePath);

                        configInfo[i].models[k].model = AssetDatabase.LoadAssetAtPath<GameObject>(modelData.prefabPath);
                        if (modelData.additionalPrefabPath != null && !modelData.additionalPrefabPath.Equals(""))
                            configInfo[i].models[k].additionalModel = AssetDatabase.LoadAssetAtPath<GameObject>(modelData.additionalPrefabPath);

                        configInfo[i].models[k].availableMeshes = modelData.availableMeshes;
                        configInfo[i].models[k].meshMask = GetAvailableMesh(modelData.availableMeshes);
                    }
                }
            }

            InitHideMask();
        }

        void SetValues(EquipItemData selectedObject)
        {
            _serializedObject.ApplyModifiedProperties();

            SetHidedMeshes();

            if (string.IsNullOrEmpty(selectedObject.guid))
                selectedObject.guid = Guid.NewGuid().ToString();

            selectedObject.itemName = itemName;
            selectedObject.hidedMeshTypes = hidedMeshes;
            selectedObject.itemType = itemType;
            selectedObject.itemSubType = itemSubType;

            if (icon != null)
            {
                selectedObject.icon = new PathData()
                {
                    path = AssetDatabase.GetAssetPath(icon),
                    bundlePath = icon.name,
                    addressPath = icon.name
                };
            }

            if (prefab != null)
            {
                selectedObject.prefab.path = AssetDatabase.GetAssetPath(prefab);
            }

            selectedObject.configsItems = new ConfigEquipItemData[configInfo.Length];
            for (int i = 0; i < configInfo.Length; i++)
            {
                var configData = new ConfigEquipItemData();
                configData.configGuid = configInfo[i].characterConfig.guid;
                configData.characterConfig = configInfo[i].characterConfig;

                configData.textures = new EquipTextureData[configInfo[i].textures.Length];
                for (int j = 0; j < configInfo[i].textures.Length; j++)
                {
                    var configTextureData = new EquipTextureData();
                    configTextureData.texturePath = AssetDatabase.GetAssetPath(configInfo[i].textures[j].texture);
                    configTextureData.textureType = configInfo[i].textures[j].textureType;

                    configData.textures[j] = configTextureData;
                }

                configData.models = new EquipModelData[configInfo[i].models.Length];
                for (int k = 0; k < configInfo[i].models.Length; k++)
                {
                    var configModelData = new EquipModelData();
                    configModelData.availableMeshes = GetAvailableMeshes(configInfo[i].models[k].meshMask);
                    configModelData.texturePath = AssetDatabase.GetAssetPath(configInfo[i].models[k].texture);
                    configModelData.prefabPath = AssetDatabase.GetAssetPath(configInfo[i].models[k].model);
                    if (configModelData.prefabPath == "")
                    {
                        var prefab = PrefabUtility.GetCorrespondingObjectFromSource(configInfo[i].models[k].model);
                        configModelData.prefabPath = AssetDatabase.GetAssetPath(prefab);
                    }

                    if (configInfo[i].models[k].additionalModel != null)
                    {
                        configModelData.additionalPrefabPath = AssetDatabase.GetAssetPath(configInfo[i].models[k].additionalModel);
                        if (configModelData.additionalPrefabPath == "")
                        {
                            var prefab = PrefabUtility.GetCorrespondingObjectFromSource(configInfo[i].models[k].additionalModel);
                            configModelData.additionalPrefabPath = AssetDatabase.GetAssetPath(prefab);
                        }
                    }

                    if (configInfo[i].models[k].additionalTexture != null)
                    {
                        configModelData.additionalTexturePath = AssetDatabase.GetAssetPath(configInfo[i].models[k].additionalTexture);
                    }

                    configData.models[k] = configModelData;
                }

                selectedObject.configsItems[i] = configData;
            }

            selectedObject.randomStats = _isRandomStats;
            selectedObject.randomStatsCount = _randomStatsCount;
        }

        private void SetHidedMeshes()
        {
            List<MeshType> hidedList = new List<MeshType>();
            foreach (var enumValue in Enum.GetValues(typeof(MeshType)))
            {
                int checkHideBit = (int) hidedMeshMask & (int) enumValue;
                if (checkHideBit != 0) hidedList.Add((MeshType) enumValue);
            }
            hidedMeshes = hidedList.ToArray();
        }


        private MeshType[] GetAvailableMeshes(MeshType meshMask)
        {
            List<MeshType> meshList = new List<MeshType>();
            foreach (var enumValue in Enum.GetValues(typeof(MeshType)))
            {
                int checkHideBit = (int)meshMask & (int)enumValue;
                if (checkHideBit != 0) meshList.Add((MeshType)enumValue);
            }
            return meshList.ToArray();
        }


        private void InitHideMask()
        {
            hidedMeshMask = 0;
            if (hidedMeshes != null)
            {
                for (int i = 0; i < hidedMeshes.Length; i++)
                    hidedMeshMask |= hidedMeshes[i];
            }
        }

        private MeshType GetAvailableMesh(MeshType[] availableMeshes)
        {
            MeshType meshMask = 0;
            if (availableMeshes != null)
            {
                for (int i = 0; i < availableMeshes.Length; i++)
                    meshMask |= availableMeshes[i];
            }

            return meshMask;
        }

        private void CreateFolders()
        {
            if (!System.IO.Directory.Exists(Application.dataPath + "/Game/Data"))
                AssetDatabase.CreateFolder("Assets/Game", "Data");

            if (!System.IO.Directory.Exists(Application.dataPath + "/Game/Data/Items"))
                AssetDatabase.CreateFolder("Assets/Game/Data", "Items");

            if (!System.IO.Directory.Exists(Application.dataPath + "/Game/Data/Items/" + itemType))
                AssetDatabase.CreateFolder("Assets/Game/Data/Items", itemType.ToString());

            if (itemSubType != EquipItemSubType.Undefined)
            {
                if (!System.IO.Directory.Exists(Application.dataPath + "/Game/Data/Items/" + itemType + "/" + itemSubType))
                    AssetDatabase.CreateFolder("Assets/Game/Data/Items/" + itemType, itemSubType.ToString());
            }
        }

        void OnWizardCreate()
        {
            var item = CreateInstance<EquipItemData>();

            SetValues(item);
            CreateFolders();

            AssetDatabase.CreateAsset(item, GetAssetPath(item.guid));
            AssetDatabase.SaveAssets();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = item;
        }

        void OnWizardUpdate()
        {
            helpString = "Enter item details";
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
            OnValidate();
        }

        private string GetAssetPath(string id)
        {
            var subType = itemSubType != EquipItemSubType.Undefined 
                ? "/" + itemSubType 
                : "";
            return "Assets/Game/Data/Items/" + itemType + subType + "/" + itemName + "_" + id +".asset";
        }
    }
}