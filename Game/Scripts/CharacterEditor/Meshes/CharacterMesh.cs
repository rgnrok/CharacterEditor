using System;
using System.Collections.Generic;
using UnityEngine;

namespace CharacterEditor
{
    namespace Mesh
    {
        public class CharacterMesh
        {
            public readonly MeshType MeshType;
            public readonly bool IsFaceMesh;

            public GameObject LoadedMeshObject { get; private set; }

            public CharacterTexture Texture => 
                _textures[SelectedMeshIndex != -1 ? SelectedMeshIndex : 0];

            public string MeshPath => 
                SelectedMeshIndex == -1 ? null : _meshPaths[SelectedMeshIndex];

            private int MeshesCount => _meshPaths?.Length ?? 0;

            // -1 without mesh
            private int _selectedMeshIndex = -1;
            public int SelectedMeshIndex
            {
                get => _selectedMeshIndex;
                private set
                {
                    value = Helper.GetActualIndex(value, MeshesCount, -1);
                    if (_selectedMeshIndex == value && LoadedMeshObject != null) return;
      
                    _prevMeshPath = MeshPath;
                    _prevMeshTextureGroup = Texture;

                    LoadedMeshObject = null;

                    _selectedMeshIndex = value;
                    if (_selectedMeshIndex != -1)
                        LoadMesh();
                }
            }

            private bool _isReady;
            public bool IsReady
            {
                get => _isReady && Texture.IsReady;
                private set => _isReady = value;
            }

            private readonly IMeshLoader _meshLoader;
            private readonly CharacterTexture[] _textures;
            private readonly string[] _meshPaths;

            private string _prevMeshPath;
            private CharacterTexture _prevMeshTextureGroup;
      
            
            public CharacterMesh(IMeshLoader loader, ITextureLoader textureLoader, Dictionary<string, string[][]> meshAndTexturesPaths, MeshType type, bool isFace)
            {
                _meshLoader = loader;
                MeshType = type;
                IsFaceMesh = isFace;


                _meshPaths = new string[meshAndTexturesPaths.Count];
                _textures = new CharacterTexture[meshAndTexturesPaths.Count];

                int i = 0;
                foreach (var meshInfo in meshAndTexturesPaths)
                {
                    _meshPaths[i] = meshInfo.Key;
                    _textures[i] = new MeshTexture(textureLoader, meshInfo.Value);
                    i++;
                }

                IsReady = true;
            }
            
            private void LoadMesh()
            {
                IsReady = false;
                _meshLoader.LoadByPath(MeshPath, (path, meshObject) =>
                {
                    LoadedMeshObject = meshObject;
                    IsReady = true;
                });
            }

            public void ClearPrevMesh()
            {
                ClearMesh(_prevMeshPath, _prevMeshTextureGroup);
            }

            private void UnsetMesh()
            {
                ClearMesh(MeshPath, Texture);
                LoadedMeshObject = null;
                SelectedMeshIndex = -1;
            }

            private void ClearMesh(string meshPath, CharacterTexture texture)
            {
                if (meshPath == null) return;

                _meshLoader.Unload(meshPath);
                texture.UnloadTexture();
            }

            public void MoveNext()
            {
                if (SelectedMeshIndex != -1 && Texture.HasNext())
                {
                    Texture.MoveNext();
                }
                else
                {
                    var color = Texture.SelectedColorIndex;
                    SelectedMeshIndex++;
                    Texture.SetTextureAndColor(0, color);
                }
            }

            public void MovePrev()
            {
                if (SelectedMeshIndex != -1 && Texture.HasPrev())
                {
                    Texture.MovePrev();
                }
                else
                {
                    var color = Texture.SelectedColorIndex;
                    SelectedMeshIndex--;
                    Texture.SetTextureAndColor(0, color);
                }
            }

            public void Reset()
            {
                UnsetMesh();
            }

            public void Shuffle(int color = -1)
            {
                SelectedMeshIndex = UnityEngine.Random.Range(-1, MeshesCount);
                if (SelectedMeshIndex == -1)
                {
                    Texture.UnloadTexture();
                    return;
                }

                if (color == -1) Texture.Shuffle();
                else Texture.ShuffleWithColor(color);
            }
            
            public void SetMesh(int mesh)
            {
                SelectedMeshIndex = mesh;
            }

            public void SetTexture(int texture)
            {
                Texture.SetTexture(texture);
            }

            public void MoveNextColor()
            {
                Texture.MoveNextColor();
            }

            public void MovePrevColor()
            {
                Texture.MovePrevColor();
            }

            public void SetColor(int color)
            {
                Texture.SetColor(color);
            }

            public void ResetColor()
            {
                Texture.ResetColor();
            }

            public void SetTextureAndColor(int texture, int color)
            {
                Texture.SetTextureAndColor(texture, color);
            }

            public bool HasChanges()
            {
                return _prevMeshPath != null && _prevMeshPath != MeshPath;
            }
        }
    }
}
