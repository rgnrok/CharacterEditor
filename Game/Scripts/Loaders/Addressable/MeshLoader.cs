using System;
using System.Threading.Tasks;
using CharacterEditor.CharacterInventory;
using UnityEngine;

namespace CharacterEditor
{
    namespace AddressableLoader
    {
        public class MeshLoader : CommonLoader<GameObject>, IMeshLoader
        {
            private readonly ITextureLoader _textureLoader;

            public MeshLoader(ITextureLoader textureLoader)
            {
                _textureLoader = textureLoader;
            }

            public void LoadItemMesh(string meshPath, string texturePath, Action<GameObject, ItemTexture> callback)
            {
                LoadByPath(meshPath, (path, mesh) =>
                {
                    ItemTexture texture = null;
                    if (texturePath != null)
                    {
                        texture = new ItemTexture(_textureLoader, texturePath);
                        texture.LoadTexture();
                    }

                    callback.Invoke(mesh, texture);
                });
            }

            public async Task<(GameObject mesh, ItemTexture texture)> LoadItemMesh(string meshPath, string texturePath)
            {
                ItemTexture texture = null;
                if (texturePath != null)
                {
                    texture = new ItemTexture(_textureLoader, texturePath);
                    texture.LoadTexture();
                }

                var mesh = await LoadByPath(meshPath);
                return (mesh, texture);
            }

            public MeshTexture CreateMeshTexture(string[][] textures)
            {
                return new MeshTexture(_textureLoader, textures);
            }
        }
    }
}