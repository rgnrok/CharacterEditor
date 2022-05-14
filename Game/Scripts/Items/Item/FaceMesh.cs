using System.Collections.Generic;
using UnityEngine;

namespace CharacterEditor
{
    namespace CharacterInventory
    {
        public class FaceMesh
        {
            public ItemMesh ItemMesh { get; private set; }
            public MeshType MeshType { get; private set; }

            public GameObject MeshInstance;
            public GameObject PreviewMeshInstance;
            public string PrefabPath { get; private set; }

            public FaceMesh(IMeshLoader loader, MeshType type, string prefabPath)
            {
                PrefabPath = prefabPath;
                MeshType = type;
                ItemMesh = FaceMeshFactory.Create(type, loader, prefabPath);
            }

            public void Equip(Dictionary<MeshType, Transform> meshBones, Dictionary<MeshType, Transform> previewMeshBones)
            {
                if (meshBones.TryGetValue(ItemMesh.MeshType, out var anchor))
                    MeshInstance = ItemMesh.InstanceMesh(anchor, 0, true);

                if (previewMeshBones != null)
                {
                    Transform previewAnchor;
                    if (previewMeshBones.TryGetValue(ItemMesh.MeshType, out previewAnchor))
                    {
                        PreviewMeshInstance = ItemMesh.InstanceMesh(previewAnchor, 0, true, true);
                        Helper.SetLayerRecursively(PreviewMeshInstance, Constants.LAYER_CHARACTER_PREVIEW);
                    }
                }
            }

            public void LoadTextureAndMesh(string guid)
            {
                ItemMesh.LoadMesh();
            }
        }
    }
}