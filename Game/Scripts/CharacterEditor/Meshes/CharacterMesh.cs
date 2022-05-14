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

            private GameObject _loadedMeshObject;
            public GameObject LoadedMeshObject => _loadedMeshObject;

            public CharacterTexture Texture => 
                _textures[SelectedMesh != -1 ? SelectedMesh : 0];

            public string MeshPath => 
                SelectedMesh == -1 ? null : _meshPaths[SelectedMesh];

            private int MeshesCount => _meshPaths?.Length ?? 0;

            // -1 without mesh
            private int _selectedMesh = -1;
            public int SelectedMesh
            {
                get => _selectedMesh;
                private set
                {
                    value = Helper.GetActualIndex(value, MeshesCount, -1);
                    if (_selectedMesh == value && _loadedMeshObject != null) return;

      
                    _prevMeshPath = MeshPath;
                    _prevMeshTexture = Texture;

                    _loadedMeshObject = null;

                    _selectedMesh = value;
                    if (_selectedMesh != -1)
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
            private CharacterTexture _prevMeshTexture;
      
            
            public CharacterMesh(IMeshLoader loader, Dictionary<string, string[][]> meshAndTexturesPaths, MeshType type, bool isFace)
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
                    _textures[i] = loader.CreateMeshTexture(meshInfo.Value);
                    i++;
                }

                IsReady = true;
            }


            private void LoadMesh()
            {
                IsReady = false;
                _meshLoader.LoadByPath(MeshPath, (path, meshObject) =>
                {
                    _loadedMeshObject = meshObject;
                    IsReady = true;
                });
            }

            public void ClearPrevMesh()
            {
                ClearMesh(_prevMeshPath, _prevMeshTexture);
            }

            private void UnsetMesh()
            {
                ClearMesh(MeshPath, Texture);
                _loadedMeshObject = null;
                SelectedMesh = -1;
            }

            private void ClearMesh(string meshPath, CharacterTexture texture)
            {
                if (meshPath == null) return;

                _meshLoader.Unload(meshPath);
                texture.UnloadTexture();

            }

            public void MoveNext()
            {
                if (SelectedMesh != -1 && Texture.HasNext())
                {
                    Texture.MoveNext();
                }
                else
                {
                    var color = Texture.SelectedColor;
                    SelectedMesh++;
                    Texture.SetTextureAndColor(0, color);
                }
            }

            public void MovePrev()
            {
                if (SelectedMesh != -1 && Texture.HasPrev())
                {
                    Texture.MovePrev();
                }
                else
                {
                    var color = Texture.SelectedColor;
                    SelectedMesh--;
                    Texture.SetTextureAndColor(0, color);
                }
            }

            public void Reset()
            {
                UnsetMesh();
            }

            public void Shuffle(int color = -1)
            {
                SelectedMesh = UnityEngine.Random.Range(-1, MeshesCount);
                if (SelectedMesh == -1) return;

                if (color == -1) Texture.Shuffle();
                else Texture.ShuffleWithColor(color);
            }


            public void SetMesh(int mesh)
            {
                SelectedMesh = mesh;
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
