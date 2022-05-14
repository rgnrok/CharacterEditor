#if UNITY_EDITOR

using System;
using UnityEngine;
using CharacterEditor.CharacterInventory;
using UnityEditor;

namespace CharacterEditor
{
    namespace AssetDatabaseLoader
    {
        public class MeshLoader : CommonLoader<GameObject>, IMeshLoader
        {
            private readonly ITextureLoader _textureLoader;

            public MeshLoader(ITextureLoader textureLoader)
            {
                _textureLoader = textureLoader;
        
            }

            public MeshTexture CreateMeshTexture(string[][] textures) =>
                new MeshTexture(_textureLoader, textures);

 

            public void LoadItemMesh(string meshPath, string texturePath, Action<GameObject, ItemTexture> callback)
            {
                var meshObject = AssetDatabase.LoadAssetAtPath<GameObject>(meshPath);
                ItemTexture texture = null;
                if (texturePath != null)
                {
                    texture = new ItemTexture(_textureLoader, texturePath);
                    texture.LoadTexture();
                }

                callback.Invoke(meshObject, texture);
            }
        }
    }
}
#endif