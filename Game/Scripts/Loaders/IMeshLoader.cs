using System;
using CharacterEditor.CharacterInventory;
using UnityEngine;

namespace CharacterEditor
{
    public interface IMeshLoader : ICommonLoader<GameObject>, IService
    {
        void LoadItemMesh(string meshPath, string texturePath, Action<GameObject, ItemTexture> callback);

        MeshTexture CreateMeshTexture(string[][] textures);
    }
}
