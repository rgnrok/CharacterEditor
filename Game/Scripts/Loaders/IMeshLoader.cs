using System;
using System.Collections.Generic;
using CharacterEditor.CharacterInventory;
using UnityEngine;

namespace CharacterEditor
{
    public interface IMeshLoader : ICommonLoader<GameObject>
    {
        void LoadItemMesh(string meshPath, string texturePath, Action<GameObject, ItemTexture> callback);

        MeshTexture CreateMeshTexture(string[][] textures);
    }
}
