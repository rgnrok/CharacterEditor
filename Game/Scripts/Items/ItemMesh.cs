using System.Threading.Tasks;
using UnityEngine;

namespace CharacterEditor
{
    namespace CharacterInventory
    {
        public class ItemMesh
        {
            public readonly MeshType MeshType;
            public readonly string MeshPath;

            public Texture2D Texture =>
                _itemTexture?.Texture;

            private readonly ItemTexture _itemTexture;

            private readonly IMeshLoader _loader;

            private GameObject _meshObject;

            public ItemMesh(MeshType type, IMeshLoader loader, string meshPath, ItemTexture itemTexture = null)
            {
                MeshType = type;
                MeshPath = meshPath;

                _loader = loader;
                _itemTexture = itemTexture;
            }

            public async Task LoadMesh()
            {
                _meshObject = await _loader.LoadByPath(MeshPath);

                if (_itemTexture != null)
                    await _itemTexture.LoadTexture();
            }

            public void UnLoadMesh()
            {
                _loader.Unload(MeshPath);
            }

     

            public GameObject InstanceMesh(Transform anchor, int layer = 0, bool active = false, bool withoutLOD = false)
            {
                var meshObject = Object.Instantiate(_meshObject, anchor.position, anchor.rotation, anchor);
                if (layer != 0) Helper.SetLayerRecursively(meshObject, layer);

                if (_itemTexture != null || withoutLOD)
                {
                    var renders = meshObject.GetComponentsInChildren<MeshRenderer>();
                    if (withoutLOD)
                    {
                        var LODGroup = meshObject.GetComponent<LODGroup>();
                        if (LODGroup != null)
                        {
                            GameObject.Destroy(LODGroup);
                            for (var i = 1; i < renders.Length; i++)
                            {
                                GameObject.Destroy(renders[i].gameObject);
                            }
                        }
                    }

                    if (_itemTexture != null)
                    {
                        foreach (var render in renders)
                            render.material.mainTexture = _itemTexture.Texture;
                    }
                }

                meshObject.SetActive(active);
                return meshObject;
            }
        }
    }
}