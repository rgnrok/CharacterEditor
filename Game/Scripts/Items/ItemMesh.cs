using UnityEngine;

namespace CharacterEditor
{
    namespace CharacterInventory
    {
        public class ItemMesh
        {
            public MeshType MeshType { get; private set; }
            public ItemTexture Texture { get; private set; }

            public readonly int AtlasPosition;

            private bool _isReady;
            public bool IsReady
            {
                get { return _isReady && (Texture == null || Texture.IsReady); }
                private set { _isReady = value; }
            }

            private string _meshPath;
            private string _texturePath;
            private GameObject _mesh;
            private IMeshLoader _loader;

            protected ItemMesh(IMeshLoader loader, string meshPath, string texturePath, MeshType type, int position)
            {
                MeshType = type;
                AtlasPosition = position;
                _loader = loader;
                _meshPath = meshPath;
                _texturePath = texturePath;
            }

            public void LoadMesh()
            {
                IsReady = false;
                _loader.LoadItemMesh(_meshPath, _texturePath, LoadingMesh);
            }

            public void UnLoadMesh()
            {
                _loader.Unload(_meshPath);
            }

            private void LoadingMesh(GameObject meshObject, ItemTexture texture)
            {
                IsReady = true;
                Texture = texture;
                _mesh = meshObject;
            }

            public GameObject InstanceMesh(Transform anchor, int layer = 0, bool active = false, bool withoutLOD = false)
            {
                var meshObject = Object.Instantiate(_mesh, anchor.position, anchor.rotation, anchor);
                if (layer != 0) Helper.SetLayerRecursively(meshObject, layer);

                if (Texture != null || withoutLOD)
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

                    if (Texture != null)
                    {
                        foreach (var render in renders)
                            render.material.mainTexture = Texture.Texture;
                    }
                }

                meshObject.SetActive(active);
                return meshObject;
            }
        }
    }
}