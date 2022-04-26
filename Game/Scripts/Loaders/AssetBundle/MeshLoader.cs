using System;
using System.Collections.Generic;
using System.Linq;
using CharacterEditor.CharacterInventory;
using Game;
using UnityEngine;

namespace CharacterEditor
{
    namespace AssetBundleLoader
    {
        public class MeshLoader : CommonLoader<GameObject>, IMeshLoader
        {
            private readonly ITextureLoader _textureLoader;

            public MeshLoader(ITextureLoader textureLoader, ICoroutineRunner coroutineRunner) : base(coroutineRunner)
            {
                _textureLoader = textureLoader;
            }

            public void LoadMesh(string meshPath, Action<string, GameObject> callback)
            {
                LoadByPath(meshPath, callback);
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

            public MeshTexture CreateMeshTexture(string[][] textures)
            {
                return new MeshTexture(_textureLoader, textures);
            }
        }
    }
}