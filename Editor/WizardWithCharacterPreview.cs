using System;
using System.Collections.Generic;
using System.Linq;
using CharacterEditor;
using CharacterEditor.CharacterInventory;
using CharacterEditor.Services;
using Editor;
using UnityEditor;
using UnityEngine;

public abstract class WizardWithCharacterPreview : ScriptableWizard
{

    protected Rect prect = new Rect();
    protected Preview preview;
    protected GameObject _currentCharacterPreview;
    protected Vector2 scrollPos;
    protected bool _updateCharacterPreview;

    private Dictionary<string, GameObject> _itemModelPreviews = new Dictionary<string, GameObject>();
    private MergeTextureService _mergeTextureService;
    private Material _mergeMat;
    private RenderTexture _renderClothTexture;
    private Texture2D _characterTexture;
    private Material _meshMaterial;


    protected abstract CharacterConfig GetSelectedCharacterConfig();
    protected abstract EquipModelData[] GetSelectedCharacterMeshes();
    protected abstract EquipTextureData[] GetSelectedCharacterTextures();

    protected void Initialize()
    {
        _mergeTextureService = new MergeTextureService();
        _mergeMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Editor/Materials/SkinAndClothRenderMaterial.mat");
        _meshMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Editor/Materials/Unlit.mat");
        _renderClothTexture = new RenderTexture(Constants.SKIN_TEXTURE_ATLAS_SIZE, Constants.SKIN_TEXTURE_ATLAS_SIZE, 1, RenderTextureFormat.ARGB32);
        _characterTexture = new Texture2D(Constants.SKIN_TEXTURE_ATLAS_SIZE, Constants.SKIN_TEXTURE_ATLAS_SIZE, TextureFormat.RGB24, false);
    }

    protected void UpdateCharacterTexturesAndMeshes(GameObject previewGameObject)
    {
        _currentCharacterPreview = previewGameObject;

        if (_currentCharacterPreview == null) return;


        foreach (var itemModelObj in _itemModelPreviews.Values)
        {
            if (itemModelObj == null) continue;
            itemModelObj.SetActive(false);
        }
        UpdateCharacterMeshes();
        UpdateCharacterTextures();
    }


    private void UpdateCharacterMeshes()
    {
        var selectedConfig = GetSelectedCharacterConfig();
        var models = GetSelectedCharacterMeshes();

        foreach (var model in models)
        {
            var availableMeshes = model.availableMeshes;
            if (availableMeshes.Length == 0) continue;

            var meshBone = selectedConfig.availableMeshes.FirstOrDefault(avMesh => avMesh.mesh == availableMeshes[0]);
            if (meshBone == null) continue;

            var uvModelPath = model.prefabPath;
            var faceTypes = new[] {MeshType.Beard, MeshType.Hair, MeshType.FaceFeature};
            if (!model.availableMeshes.Any(type => faceTypes.Contains(type)))
                uvModelPath = uvModelPath.Replace("/StaticModel/", "/Model/");

            var bone = _currentCharacterPreview.transform.FindTransform(meshBone.boneName);
            if (bone == null) continue;

            var uvPrefab = GetMeshPrefab(uvModelPath);
            var meshMaterial = new Material(_meshMaterial);
            foreach (var meshRender in uvPrefab.GetComponentsInChildren<MeshRenderer>())
            {
                meshMaterial.mainTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(model.texturePath);
                meshRender.material = meshMaterial;
            }

            uvPrefab.transform.parent = bone;
            uvPrefab.transform.localPosition = Vector3.zero;
            uvPrefab.transform.localRotation = Quaternion.identity;
        }
    }

    private GameObject GetMeshPrefab(string uvModelPath)
    {
        GameObject uvPrefab;
        if (_itemModelPreviews.ContainsKey(uvModelPath))
        {
            uvPrefab = _itemModelPreviews[uvModelPath];
            uvPrefab.SetActive(true);
        }
        else
        {
            var uvModel = AssetDatabase.LoadAssetAtPath<GameObject>(uvModelPath);
            uvPrefab = (GameObject) PrefabUtility.InstantiatePrefab(uvModel);
            _itemModelPreviews[uvModelPath] = uvPrefab;
        }

        return uvPrefab;
    }

    private void UpdateCharacterTextures()
    {
        var textures = GetSelectedCharacterTextures();
        var textureDic = new Dictionary<string, Texture2D>(textures.Length);

        foreach (var texture in textures)
        {
            var key = Helper.GetShaderTextureName(texture.textureType);
            textureDic[key] = AssetDatabase.LoadAssetAtPath<Texture2D>(texture.texturePath);
        }

        _mergeTextureService.MergeTextures(_mergeMat, _renderClothTexture, textureDic);

        RenderTexture.active = _renderClothTexture;
        _characterTexture.ReadPixels(new Rect(0, 0, _renderClothTexture.width, _renderClothTexture.height), 0, 0);
        _characterTexture.Apply();

        foreach (var render in _currentCharacterPreview.GetComponentsInChildren<SkinnedMeshRenderer>())
            render.sharedMaterial.mainTexture = _characterTexture;
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

}
