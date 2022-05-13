using System;
using System.Collections.Generic;
using CharacterEditor;
using CharacterEditor.CharacterInventory;
using Editor;
using UnityEditor;
using UnityEngine;

public abstract class WizardWithCharacterPreview : ScriptableWizard
{


    protected int _currentModelAndTextureHash;
    protected int _prevModelAndTextureHash;
    protected Rect prect = new Rect();
    protected Preview p;
    protected GameObject _currentCharacterPreview;
    protected Vector2 scrollPos;
    protected bool _updateCharacterPreview;

    private Dictionary<string, GameObject> _itemModelPreviews = new Dictionary<string, GameObject>();





    protected abstract void GenerateCharacterPreview();

    protected abstract CharacterConfig GetSelectedCharacterConfig();
    protected abstract EquipModelData[] GetSelectedCharacterMeshes();
    protected abstract EquipTextureData[] GetSelectedCharacterTextures();

    protected void UpdateCharacterTexturesAndMeshes()
    {
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

            MeshTypeBone meshBone = null;
            foreach (var avMesh in selectedConfig.availableMeshes)
            {
                if (avMesh.mesh != availableMeshes[0]) continue;
                meshBone = avMesh;
                break;
            }

            if (meshBone == null) continue;

            var uvModelPath = model.prefabPath;
//            if (!(Array.IndexOf(model.availableMeshes, MeshType.Beard) != -1 || Array.IndexOf(model.availableMeshes, MeshType.Hair) != -1) || Array.IndexOf(model.availableMeshes, MeshType.FaceFeature) != -1)
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
                uvPrefab = (GameObject) PrefabUtility.InstantiatePrefab(uvModel);
                _itemModelPreviews[uvModelPath] = uvPrefab;
            }

            foreach (var meshRender in uvPrefab.GetComponentsInChildren<MeshRenderer>())
            {
                meshRender.sharedMaterial.mainTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(model.texturePath);
            }

            var bone = Helper.FindTransform(_currentCharacterPreview.transform, meshBone.boneName);
            if (bone != null)
            {
                uvPrefab.transform.parent = bone;
                uvPrefab.transform.localPosition = Vector3.zero;
                uvPrefab.transform.localRotation = Quaternion.identity;
            }
        }
    }

    private void UpdateCharacterTextures()
    {
        var textures = GetSelectedCharacterTextures();
        var renderClothTexture = new RenderTexture(1024, 1024, 1, RenderTextureFormat.ARGB32);

        var emptyText = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        emptyText.SetPixel(0, 0, new Color(1, 1, 1, 0));
        emptyText.Apply();
        var mat = AssetDatabase.LoadAssetAtPath<Material>(
            "Assets/Editor/SkinAndClothRenderMaterial.mat");

        foreach (TextureType type in Enum.GetValues(typeof(TextureType)))
        {
            mat.SetTexture(Helper.GetShaderTextureName(type), emptyText);
        }

        foreach (var texture in textures)
        {
            mat.SetTexture(Helper.GetShaderTextureName(texture.textureType),
                AssetDatabase.LoadAssetAtPath<Texture2D>(texture.texturePath));
        }

        Graphics.Blit(Texture2D.blackTexture, renderClothTexture, mat);

        RenderTexture.active = renderClothTexture;
        var characterTexture = new Texture2D(Constants.SKIN_TEXTURE_ATLAS_SIZE, Constants.SKIN_TEXTURE_ATLAS_SIZE,
            TextureFormat.RGB24, false);
        characterTexture.ReadPixels(new Rect(0, 0, renderClothTexture.width, renderClothTexture.height), 0, 0);
        characterTexture.Apply();

        foreach (var render in _currentCharacterPreview.GetComponentsInChildren<SkinnedMeshRenderer>())
        {
            render.sharedMaterial.mainTexture = characterTexture;
        }

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
