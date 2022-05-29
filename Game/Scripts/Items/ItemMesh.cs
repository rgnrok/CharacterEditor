using UnityEngine;

namespace CharacterEditor
{
    namespace CharacterInventory
    {
        public class ItemMesh
        {
            public readonly MeshType MeshType;
            public readonly string MeshPath;

            private bool _isReady;

            public bool IsReady
            {
                get => _isReady && (_texture == null || _texture.IsReady);
                private set => _isReady = value;
            }


            private ItemTexture _texture;
            private readonly string _texturePath;
            private readonly IMeshLoader _loader;

            private GameObject _mesh;

            public ItemMesh(MeshType type, IMeshLoader loader, string meshPath, string texturePath = null)
            {
                MeshType = type;
                _loader = loader;
                MeshPath = meshPath;
                _texturePath = texturePath;
            }

            public void LoadMesh()
            {
                IsReady = false;
                _loader.LoadItemMesh(MeshPath, _texturePath, LoadingMesh);
            }

            public void UnLoadMesh()
            {
                _loader.Unload(MeshPath);
            }

            private void LoadingMesh(GameObject meshObject, ItemTexture texture)
            {
                IsReady = true;
                _texture = texture;
                _mesh = meshObject;
            }

            public GameObject InstanceMesh(Transform anchor, int layer = 0, bool active = false, bool withoutLOD = false)
            {
                var meshObject = Object.Instantiate(_mesh, anchor.position, anchor.rotation, anchor);
                if (layer != 0) Helper.SetLayerRecursively(meshObject, layer);

                if (_texture != null || withoutLOD)
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

                    if (_texture != null)
                    {
                        foreach (var render in renders)
                            render.material.mainTexture = _texture.Texture;
                    }
                }

                meshObject.SetActive(active);
                return meshObject;
            }
        }
    }
}