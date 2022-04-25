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
            private readonly LoadedDataManager _dataManager;

            public MeshLoader(ITextureLoader textureLoader, LoadedDataManager dataManager, ICoroutineRunner coroutineRunner) : base(coroutineRunner)
            {
                _textureLoader = textureLoader;
                _dataManager = dataManager;
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

            public Dictionary<string, string[][]> ParseMeshes(string characterRace, MeshType meshType)
            {
                if (!_dataManager.RaceMeshes.TryGetValue(characterRace, out var raceMeshesMap)) return null;
                if (!raceMeshesMap.TryGetValue(meshType, out var meshesMap)) return null;

                return meshesMap.meshPaths.ToDictionary(
                    x => x.modelPath,
                    x =>
                    {
                        var texturePaths = new string[x.textures.Count][];
                        for (var i = 0; i < x.textures.Count; i++)
                            texturePaths[i] = x.textures[i].colorPaths.ToArray();

                        return texturePaths;
                    }
                );
            }
        }
    }
}