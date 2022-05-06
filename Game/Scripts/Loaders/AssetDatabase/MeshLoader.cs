#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEngine;
using CharacterEditor.CharacterInventory;
using UnityEditor;

namespace CharacterEditor
{
    namespace AssetDatabaseLoader
    {
        /*
         * Parse and Load Meshes (Only editor)
         */
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
                var meshObject = AssetDatabase.LoadAssetAtPath(meshPath, typeof(GameObject)) as GameObject;
                var texture = new ItemTexture(_textureLoader, texturePath);

                callback.Invoke(meshObject, texture);
            }
            
        
        }
    }
}
#endif