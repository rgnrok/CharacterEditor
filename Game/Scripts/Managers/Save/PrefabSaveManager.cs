#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CharacterEditor.Services;
using UnityEditor;
using UnityEngine;


namespace CharacterEditor
{
    public class PrefabSaveManager
    {
        private readonly TextureManager _textureManager;
        private readonly MeshManager _meshManager;
        private readonly IStaticDataService _staticDataService;
        private readonly CharacterConfig _config;
        private readonly CharacterGameObjectData _goData;

        public PrefabSaveManager(TextureManager textureManager, MeshManager meshManager, IStaticDataService staticDataService, CharacterConfig config, CharacterGameObjectData goData)
        {
            _textureManager = textureManager;
            _meshManager = meshManager;
            _staticDataService = staticDataService;
            _config = config;
            _goData = goData;
        }

        private string CreateCharacterFolder()
        {
            var prefabNum = System.DateTime.Now.ToString("yyyy_MM_dd_HHmmss");

            var folderName = $"{_config.folderName}_{prefabNum}";
            var folderPath = $"{AssetsConstants.CharacterEditorRootPath}/NewCharacter/{_config.folderName}/{folderName}";

            Helper.TryCreateFolder(folderPath);

            // folderPath += '/' + folderName;
            // AssetDatabase.CreateFolder(folderPath, "Mesh");
            return folderPath;
        }

        public void Save()
        {
            var folder = CreateCharacterFolder();

            if (_staticDataService.MeshAtlasType != MeshAtlasType.Dynamic)
            {
                SaveCharacterPrefab(folder);
                return;
            }

            CreateArmorAtlas(folder);
            CreateFaceAtlas(folder);
            CreateCloakAtlas(folder);
            CreateSkinAtlas(folder);
            CreatePrefab(folder, _config.folderName);


            // quit the editor because the mesh you export have now new UW'S and quitting the editor play mode will remove the new UW's 
            EditorApplication.isPlaying = false;
        }

        private void SaveCharacterPrefab(string folder)
        {
            SetupPrefabMaterialAndTexture(folder, "Armor", _meshManager.ArmorTexture, GetRenderers(_meshManager.SelectedArmorMeshes));
            SetupPrefabMaterialAndTexture(folder, "Face", _meshManager.FaceTexture, GetRenderers(_meshManager.SelectedSkinMeshes));

            CreateCloakAtlas(folder);
            CreateSkinAtlas(folder);
            SaveBoneData(folder);

            CreatePrefab(folder, _config.folderName);
        }

        private void CreatePrefab(string folder, string name)
        {
            PrefabUtility.SaveAsPrefabAsset(_goData.CharacterObject, $"{folder}/{name}.prefab");
        }

        private void SetupPrefabMaterialAndTexture(string folder, string matName, Texture2D texture, IEnumerable<Renderer> renderers)
        {
            var material = GetObjectMaterial();
            AssetDatabase.CreateAsset(material, $"{folder}/mat_{matName}.mat");

            var texturePath = $"{folder}/tex_{matName}.png";
            File.WriteAllBytes(texturePath, texture.EncodeToPNG());

            AssetDatabase.Refresh();

            material.mainTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);

            foreach (var renderer in renderers)
                UpdateMeshMaterials(renderer, material);
        }

        private void CreateCloakAtlas(string folder)
        {
            var cloakTexture = _textureManager.CloakTexture;
            if (cloakTexture == null) return;

            if (!_goData.CloakMeshes.Any(mesh => mesh.gameObject.activeSelf))
                return;

            SetupPrefabMaterialAndTexture(folder, "Cloak", cloakTexture, _goData.CloakMeshes);
        }

        private void CreateSkinAtlas(string folder)
        {
            var modelRenderers = _goData.SkinMeshes
                .Concat(_goData.ShortRobeMeshes)
                .Concat(_goData.LongRobeMeshes);
          
            SetupPrefabMaterialAndTexture(folder, "Skin", _textureManager.CharacterTexture, modelRenderers);
        }

        private void SaveBoneData(string folder)
        {
            var bones = ScriptableObject.CreateInstance<PrefabBoneData>();

            bones.armorBones = GetBoneData(_meshManager.SelectedArmorMeshes);
            bones.faceBones = GetBoneData(_meshManager.SelectedSkinMeshes);

            AssetDatabase.CreateAsset(bones, folder + "/prefabBones.asset");
            AssetDatabase.SaveAssets();

        }

        private PrefabBoneData.BoneData[] GetBoneData(IReadOnlyCollection<CharacterMeshWrapper> meshWrappers)
        {
            var boneList = new List<PrefabBoneData.BoneData>(meshWrappers.Count);
            foreach (var meshWrapper in meshWrappers)
            {
                var mesh = meshWrapper.Mesh;
                if (!_goData.meshBones.TryGetValue(mesh.MeshType, out var bone)) continue;

                var assetPath = AssetDatabase.GetAssetPath(mesh.LoadedMeshObject);
                var fileName = Path.GetFileName(assetPath);
                fileName = fileName.Substring(0, fileName.IndexOf('.'));
                var bundleName = AssetImporter.GetAtPath(assetPath).assetBundleName;

                var boneData = new PrefabBoneData.BoneData();
                boneData.bone = bone.name;
                boneData.prefabPath = new PathData
                {
                    path = assetPath,
                    bundlePath = $"{bundleName}/{fileName}"
                };
                boneList.Add(boneData);
            }

            return boneList.ToArray();
        }


        private void CreateArmorAtlas(string folderName)
        {
            var materialArmor = GetObjectMaterial();
            AssetDatabase.CreateAsset(materialArmor, folderName + "/" + _config.folderName + "_ArmorMat.mat");
            AssetDatabase.Refresh();

            var armorAtlas = MeshManager.Instance.ArmorTexture;

            if (armorAtlas == null)
                return;

            int atlasSize = armorAtlas.width / Constants.MESH_TEXTURE_SIZE;
            float uvsStep = 1f / atlasSize;

            var selectedMeshes = MeshManager.Instance.SelectedArmorMeshes;
            var meshes = new Dictionary<string, MeshFilter>();

            var itemNum = 0;
            foreach (var selectedMeshWrapper in selectedMeshes)
            {
                //Update LOD parts for each armor item
                var armorsParts = selectedMeshWrapper.MeshInstance.GetComponentsInChildren<MeshFilter>();
                for (var armLOD = 0; armLOD < armorsParts.Length; armLOD++)
                {
                    if (armorsParts[armLOD] != null)
                    {
                        if (!meshes.ContainsKey(armorsParts[armLOD].name))
                        {
                            UpdateMeshMaterials(armorsParts[armLOD].GetComponent<Renderer>(), materialArmor);

                            //Update UVS for new atlas
                            Vector2[] uvs = armorsParts[armLOD].GetComponent<MeshFilter>().mesh.uv;
                            for (int i = 0; i < uvs.Length; i++)
                            {
                                uvs[i] = new Vector2(uvs[i].x / atlasSize + uvsStep * (itemNum % atlasSize),
                                    uvs[i].y / atlasSize + uvsStep * (atlasSize - 1 - (itemNum / atlasSize)));
                            }

                            armorsParts[armLOD].mesh.uv = uvs;

                            //assigne the selected LOD Mesh with new UV's to the new mesh to be exported
                            AssetDatabase.CreateAsset(
                                armorsParts[armLOD].mesh, folderName + "/Mesh/" + armorsParts[armLOD].name + "_New" + armLOD + ".asset"
                            ); //exporte asset to new project folder

                            meshes.Add(armorsParts[armLOD].name, armorsParts[armLOD]);
                        }
                        else
                        {
                            armorsParts[armLOD].mesh = meshes[armorsParts[armLOD].name].mesh;
                            UpdateMeshMaterials(armorsParts[armLOD].GetComponent<Renderer>(), materialArmor);
                        }
                    }
                }
            }
            var texturPath = folderName + "/" + _config.folderName + "_Armor.png";
            File.WriteAllBytes(texturPath, armorAtlas.EncodeToPNG());
            AssetDatabase.Refresh();
            materialArmor.mainTexture = AssetDatabase.LoadAssetAtPath(texturPath, typeof(Texture2D)) as Texture2D;
        }

        private void CreateFaceAtlas(string folderName)
        {
            var material = GetObjectMaterial();
            AssetDatabase.CreateAsset(material, folderName + "/" + _config.folderName + "_FaceMat.mat");
            AssetDatabase.Refresh();

            var faceAtlas = MeshManager.Instance.FaceTexture;
            if (faceAtlas == null) return;

            var atlasSize = faceAtlas.width / Constants.MESH_TEXTURE_SIZE;
            var uvsStep = 1f / atlasSize;

            var selectedMeshes = MeshManager.Instance.SelectedSkinMeshes;
            var meshes = new Dictionary<string, MeshFilter>();

            var itemNum = 0;
            foreach (var meshWrapper in selectedMeshes)
            {
                //Update LOD parts for each armor item
                var armorsParts = meshWrapper.MeshInstance.GetComponentsInChildren<MeshFilter>();
                for (var armLOD = 0; armLOD < armorsParts.Length; armLOD++)
                {
                    if (armorsParts[armLOD] != null)
                    {
                        if (!meshes.ContainsKey(armorsParts[armLOD].name))
                        {
                            armorsParts[armLOD].GetComponent<Renderer>().material = material;


                            //Update UVS for new atlas
                            Vector2[] uvs = armorsParts[armLOD].GetComponent<MeshFilter>().mesh.uv;
                            for (int i = 0; i < uvs.Length; i++)
                            {
                                uvs[i] = new Vector2(uvs[i].x / atlasSize + uvsStep * (itemNum % atlasSize),
                                    uvs[i].y / atlasSize + uvsStep * (atlasSize - 1 - (itemNum / atlasSize)));
                            }

                            armorsParts[armLOD].mesh.uv = uvs;

                            //assigne the selected LOD Mesh with new UV's to the new mesh to be exported
                            AssetDatabase.CreateAsset(
                                armorsParts[armLOD].mesh, folderName + "/Mesh/" + armorsParts[armLOD].name + "_New" + armLOD + ".asset"
                            ); //exporte asset to new project folder

                            meshes.Add(armorsParts[armLOD].name, armorsParts[armLOD]);
                        }
                        else
                        {
                            armorsParts[armLOD].mesh = meshes[armorsParts[armLOD].name].mesh;
                            armorsParts[armLOD].GetComponent<Renderer>().material = material;
                        }
                    }
                }
            }
            var texturePath = folderName + "/" + _config.folderName + "_Face.png";
            File.WriteAllBytes(texturePath, faceAtlas.EncodeToPNG());
            AssetDatabase.Refresh();
            material.mainTexture = AssetDatabase.LoadAssetAtPath(texturePath, typeof(Texture2D)) as Texture2D;
        }

        private Material GetObjectMaterial()
        {
            var material = PrefabShaderManager.Instance.GetShaderMaterial();
            return new Material(material.GetMaterial(MaterialType.Skin));
        }

        private IEnumerable<MeshRenderer> GetRenderers(IEnumerable<CharacterMeshWrapper> meshWrappers)
        {
            return meshWrappers.Where(meshWrapper => meshWrapper != null)
                .SelectMany(meshWrapper => meshWrapper.MeshRenders);
        }

        private void UpdateMeshMaterials(Renderer render, Material newMaterial)
        {
            var materials = render.materials;
            for (var i = 0; i < materials.Length; i++)
                materials[i] = newMaterial;

            render.materials = materials;
        }
    }
}
#endif
