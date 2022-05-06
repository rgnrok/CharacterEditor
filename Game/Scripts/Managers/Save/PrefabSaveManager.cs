#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using CharacterEditor.Mesh;
using CharacterEditor.Services;
using UnityEditor;
using UnityEngine;


namespace CharacterEditor
{
    public class PrefabSaveManager
    {
        private readonly IStaticDataService _staticDataService;
        private readonly IConfigManager _configManager;

        public PrefabSaveManager() //todo move as parameter
        {
            _staticDataService = AllServices.Container.Single<IStaticDataService>();
            _configManager = AllServices.Container.Single<IConfigManager>();
        }

        /*
         * Create Prefab folder for a character
         */
        private string CreateFolder()
        {
            var config = _configManager.Config;
            var prefabNum = System.DateTime.Now.ToString("yyyyMMddHHmmssfff");

            if (!System.IO.Directory.Exists(Application.dataPath + "/Packages/Character_Editor/NewCharacter"))
                AssetDatabase.CreateFolder(AssetsConstants.CharacterEditorRootPath, "/NewCharacter");

            if (!System.IO.Directory.Exists(
                Application.dataPath + "/Packages/Character_Editor/NewCharacter/" + config.folderName))
                AssetDatabase.CreateFolder(AssetsConstants.CharacterEditorRootPath + "/NewCharacter", config.folderName);

            var folderPath = AssetsConstants.CharacterEditorRootPath + "/NewCharacter/" + config.folderName;
            var folderName = config.folderName + "_" + prefabNum;

            AssetDatabase.CreateFolder(folderPath, folderName);

            folderPath += '/' + folderName;
            AssetDatabase.CreateFolder(folderPath, "Mesh");
            return folderPath;
        }

        public void Save()
        {
            var folderName = CreateFolder();

            if (_staticDataService.LoaderType == LoaderType.AssetDatabase &&
                _staticDataService.MeshAtlasType == MeshAtlasType.Dynamic)
            {
                CreateArmorAtlas(folderName);
                CreateFaceAtlas(folderName);
                CreateCloakAtlas(folderName);
                CreateSkinAtlas(folderName);

                // quit the editor because the mesh you export have now new UW'S and quitting the editor play mode will remove the new UW's 
                EditorApplication.isPlaying = false;
            }
            else
            {
                CreatePrefab(folderName);
            }
        }

        private void CreateSkinAtlas(string folderName)
        {
            var config = _configManager.Config;
            var race = _configManager.Config.folderName;
            var materialSkin = GetObjectMaterial();
            AssetDatabase.CreateAsset(materialSkin, folderName + "/" + race + "_SkinMat.mat");

            Texture2D mergedTexture = TextureManager.Instance.CharacterTexture;

            var texturPath = folderName + "/" + race + "_CharacterSkin.png";
            File.WriteAllBytes(texturPath, mergedTexture.EncodeToPNG());
            AssetDatabase.Refresh();
            materialSkin.mainTexture = AssetDatabase.LoadAssetAtPath(texturPath, typeof(Texture2D)) as Texture2D;

            TextureManager.Instance.UpdateMaterial(materialSkin);


            PrefabUtility.SaveAsPrefabAsset(_configManager.ConfigData.CharacterObject, folderName + "/" + race + ".prefab");
        }

        private void SaveBoneData(string folderName)
        {
            var goData = _configManager.ConfigData;
            var bones = ScriptableObject.CreateInstance<PrefabBoneData>();
            var boneList = new List<PrefabBoneData.BoneData>();

            foreach (var meshWrapper in MeshManager.Instance.SelectedArmorMeshes)
            {
                var mesh = meshWrapper.Mesh;
                if (!goData.meshBones.TryGetValue(mesh.MeshType, out var bone)) return;

                var boneData = new PrefabBoneData.BoneData();
                boneData.bone = bone.name;
                //                boneData.prefabPath = AssetDatabase.GetAssetPath(prefab);
                boneData.prefabBundlePath = mesh.MeshPath;
                boneList.Add(boneData);
            }
            bones.armorBones = boneList.ToArray();

            boneList.Clear();
            foreach (var meshWrapper in MeshManager.Instance.SelectedSkinMeshes)
            {
                var mesh = meshWrapper.Mesh;
                if (!goData.meshBones.TryGetValue(mesh.MeshType, out var bone)) return;

                var boneData = new PrefabBoneData.BoneData();
                boneData.bone = bone.name;
                boneData.prefabBundlePath = mesh.MeshPath;
                boneList.Add(boneData);
            }
            bones.faceBones = boneList.ToArray();


            AssetDatabase.CreateAsset(bones, folderName + "/prefabBones.asset");
            AssetDatabase.SaveAssets();

        }

        /*
         * Create prefab for static atlas
         */
        private void CreatePrefab(string folderName)
        {
            var config = _configManager.Config;
            var materialArmor = GetObjectMaterial();
            var a = 1;
            //            return;
            AssetDatabase.CreateAsset(materialArmor, folderName + "/" + config.folderName + "_ArmorMat.mat");
            var armorAtlas = MeshManager.Instance.ArmorTexture;
            var texturPath = folderName + "/" + config.folderName + "_Armor.png";
            File.WriteAllBytes(texturPath, armorAtlas.EncodeToPNG());
            AssetDatabase.Refresh();
            materialArmor.mainTexture = AssetDatabase.LoadAssetAtPath(texturPath, typeof(Texture2D)) as Texture2D;
            SetMeshesMaterial(materialArmor, MeshManager.Instance.SelectedArmorMeshes);

            var materialFace = GetObjectMaterial();
            AssetDatabase.CreateAsset(materialFace, folderName + "/" + config.folderName + "_FaceMat.mat");
            var faceAtlas = MeshManager.Instance.FaceTexture;
            var faceTexturPath = folderName + "/" + config.folderName + "_Face.png";
            File.WriteAllBytes(faceTexturPath, faceAtlas.EncodeToPNG());
            AssetDatabase.Refresh();
            materialFace.mainTexture = AssetDatabase.LoadAssetAtPath(faceTexturPath, typeof(Texture2D)) as Texture2D;
            SetMeshesMaterial(materialFace, MeshManager.Instance.SelectedSkinMeshes);

            CreateCloakAtlas(folderName);
            CreateSkinAtlas(folderName);
            SaveBoneData(folderName);
        }

        private void SetMeshesMaterial(Material material, IEnumerable<CharacterMeshWrapper> mesheWrapperss)
        {
            foreach (var meshWrapper in mesheWrapperss)
            {
                if (meshWrapper == null) continue;
                foreach (var meshRenderer in meshWrapper.MeshInstance.GetComponentsInChildren<MeshRenderer>())
                    UpdateMeshMaterials(meshRenderer, material);
            }
        }

        /*
         * Forming a dynamic atlas for armor. Convert a model UV
         */
        private void CreateArmorAtlas(string folderName)
        {
            var config = _configManager.Config;
            var materialArmor = GetObjectMaterial();
            AssetDatabase.CreateAsset(materialArmor, folderName + "/" + config.folderName + "_ArmorMat.mat");
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
            var texturPath = folderName + "/" + config.folderName + "_Armor.png";
            File.WriteAllBytes(texturPath, armorAtlas.EncodeToPNG());
            AssetDatabase.Refresh();
            materialArmor.mainTexture = AssetDatabase.LoadAssetAtPath(texturPath, typeof(Texture2D)) as Texture2D;
        }

        private void UpdateMeshMaterials(Renderer render, Material armorMat)
        {
            var materials = new List<Material>();
            render.GetMaterials(materials);
            for (var i = 0; i < materials.Count; i++)
                materials[i] = armorMat;

            render.materials = materials.ToArray();
        }

        private void CreateFaceAtlas(string folderName)
        {
            var config = _configManager.Config;
            var material = GetObjectMaterial();
            AssetDatabase.CreateAsset(material, folderName + "/" + config.folderName + "_FaceMat.mat");
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
            var texturPath = folderName + "/" + config.folderName + "_Face.png";
            File.WriteAllBytes(texturPath, faceAtlas.EncodeToPNG());
            AssetDatabase.Refresh();
            material.mainTexture = AssetDatabase.LoadAssetAtPath(texturPath, typeof(Texture2D)) as Texture2D;
        }

        protected void CreateCloakAtlas(string folderName)
        {
            var config = _configManager.Config;
            Texture2D cloak = TextureManager.Instance.CloakTexture;
            if (cloak == null) return;

            var cloakText = new Texture2D(cloak.width, cloak.height);
            cloakText.SetPixels32(cloak.GetPixels32());
            cloakText.Apply();

            var materialCloak = GetObjectMaterial();
            AssetDatabase.CreateAsset(materialCloak, folderName + "/" + config.folderName + "_Cloak.mat");
            var texturPath = folderName + "/" + config.folderName + "_Cloak.png";

            File.WriteAllBytes(texturPath, cloakText.EncodeToPNG());
            AssetDatabase.Refresh();
            materialCloak.mainTexture = AssetDatabase.LoadAssetAtPath(texturPath, typeof(Texture2D)) as Texture2D;
            PrefabShaderManager.Instance.UpdateCloakMaterial(materialCloak);
        }

        private Material GetObjectMaterial()
        {
            var material = PrefabShaderManager.Instance.GetShaderMaterial();
            return new Material(material.GetMaterial(MaterialType.Skin));
        }

    }
}
#endif
