using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace CharacterEditor
{
    namespace CharacterInventory
    {
        public class FaceMesh
        {
            public MeshType MeshType => _itemMesh.MeshType;
            public string MeshPath => _itemMesh.MeshPath;

            public GameObject MeshInstance;
            public GameObject PreviewMeshInstance;

            private readonly ItemMesh _itemMesh;

            public FaceMesh(ItemMesh itemMesh)
            {
                _itemMesh = itemMesh;
            }

            public void Equip(Dictionary<MeshType, Transform> meshBones, Dictionary<MeshType, Transform> previewMeshBones)
            {
                if (meshBones.TryGetValue(MeshType, out var anchor))
                    MeshInstance = _itemMesh.InstantiateMesh(anchor, 0, true);

                if (previewMeshBones != null)
                {
                    if (previewMeshBones.TryGetValue(MeshType, out var previewAnchor))
                    {
                        PreviewMeshInstance = _itemMesh.InstantiateMesh(previewAnchor, 0, true);
                        Helper.SetLayerRecursively(PreviewMeshInstance, Constants.LAYER_CHARACTER_PREVIEW);
                    }
                }
            }

            public Task LoadTextureAndMesh()
            {
                return _itemMesh.LoadMesh();
            }
        }
    }
}
