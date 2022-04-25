using System;
using System.Collections.Generic;
using CharacterEditor.CharacterInventory;
using UnityEngine;

namespace CharacterEditor
{
    public interface IMeshLoader : ICommonLoader<GameObject>
    {
        Dictionary<string, string[][]> ParseMeshes(string characterRace, MeshType meshType);

        void LoadItemMesh(string meshPath, string texturePath, Action<GameObject, ItemTexture> callback);

        MeshTexture CreateMeshTexture(string[][] textures);
    }
}
