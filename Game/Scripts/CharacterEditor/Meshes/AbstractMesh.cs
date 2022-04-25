using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CharacterEditor
{
    namespace Mesh
    {
        public abstract class AbstractMesh
        {
            private readonly IMeshLoader _meshLoader;
            private readonly Transform _anchor;


            public virtual bool IsFaceMesh => false;
            public readonly int MergeOrder; //Order in static atlas
            public readonly MeshType MeshType;

            private readonly AbstractTexture[] _textures;

            public AbstractTexture Texture => 
                _textures[SelectedMesh != -1 ? SelectedMesh : 0];


            private string _lastLoadPath;
            private readonly string[] _meshPaths;

            public string MeshPath => 
                SelectedMesh == -1 ? null : _meshPaths[SelectedMesh];


            private int MeshesCount => _meshPaths?.Length ?? 0;

            private string _prevMeshPath;
            private AbstractTexture _prevMeshTexture;
            private GameObject _prevMesh;
            public GameObject CurrentMesh { get; private set; }


            private int _selectedMesh = -1;

            public int SelectedMesh
            {
                get { return _selectedMesh; }
                private set
                {
                    if (value >= MeshesCount)
                        value = -1;

                    if (value < -1)
                        value = MeshesCount - 1;

                    if (_selectedMesh == value && CurrentMesh != null) return;

                    _prevMesh = CurrentMesh;
                    _prevMeshPath = MeshPath;
                    _prevMeshTexture = Texture;

                    CurrentMesh = null;

                    _selectedMesh = value;
                    if (_selectedMesh != -1) LoadMesh();
                }
            }

            private bool _isReady;

            public bool IsReady
            {
                get => _isReady && Texture.IsReady;
                private set => _isReady = value;
            }

            protected AbstractMesh(IMeshLoader loader, Transform anchor, string characterRace, MeshType type, int order)
            {
                _meshLoader = loader;
                _anchor = anchor;
                MeshType = type;
                MergeOrder = order;


                var meshAndTextures = _meshLoader.ParseMeshes(characterRace, type);
                _meshPaths = new string[meshAndTextures.Count];
                _textures = new AbstractTexture[meshAndTextures.Count];

                int i = 0;
                foreach (var meshInfo in meshAndTextures)
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
                _lastLoadPath = MeshPath;
                _meshLoader.LoadByPath(MeshPath, LoadMeshCallBack);
            }

            private void LoadMeshCallBack(string path, GameObject meshObject)
            {
                CurrentMesh = GameObject.Instantiate(meshObject, _anchor);
                CurrentMesh.SetActive(false);
                if (_lastLoadPath.Equals(path)) IsReady = true;
            }

            public void UpdateMesh()
            {
                if (_prevMesh != null)
                {
                    _prevMesh.SetActive(false);
                    Object.Destroy(_prevMesh);
                    _meshLoader.Unload(_prevMeshPath);
                    _prevMeshTexture.UnloadTexture();
                    _prevMesh = null;
                }

                if (CurrentMesh == null) return;

                CurrentMesh.transform.SetParent(_anchor);
                CurrentMesh.transform.position = _anchor.position;
                CurrentMesh.transform.rotation = _anchor.rotation;

                foreach (var render in CurrentMesh.GetComponentsInChildren<MeshRenderer>())
                    if (render.material != null) render.material.mainTexture = Texture.Current;
            }

            private void UnsetMesh()
            {
                if (CurrentMesh != null)
                {
                    CurrentMesh.SetActive(false);
                    _meshLoader.Unload(MeshPath);
                    Texture.UnloadTexture();
                    GameObject.Destroy(CurrentMesh);
                    CurrentMesh = null;
                }

                SelectedMesh = -1;
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
                    //Show first texture for mesh
                    Texture.SetTextureAndColor(0, color);
                }

                UpdateTextureListeners();
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
                    //Show last texture for mesh
                    Texture.SetTextureAndColor(-1, color);
                }

                UpdateTextureListeners();
            }

            public void Reset()
            {
                UnsetMesh();
            }

            public void Shuffle(int color = -1)
            {
                SelectedMesh = UnityEngine.Random.Range(-1, MeshesCount);
       
                if (color == -1) Texture.Shuffle();
                else Texture.ShuffleWithColor(color);

                UpdateTextureListeners();
            }


            public void SetMesh(int mesh)
            {
                SelectedMesh = mesh;
                UpdateTextureListeners();
            }

            public void SetTexture(int texture)
            {
                Texture.SetTexture(texture);
                UpdateTextureListeners();
            }

            public void MoveNextColor()
            {
                Texture.MoveNextColor();
                UpdateTextureListeners();
            }

            public void MovePrevColor()
            {
                Texture.MovePrevColor();
                UpdateTextureListeners();
            }

            public void SetColor(int color)
            {
                Texture.SetColor(color);
                UpdateTextureListeners();
            }

            public void ResetColor()
            {
                Texture.ResetColor();
                UpdateTextureListeners();
            }

            public void SetTextureAndColor(int texture, int color)
            {
                Texture.SetTextureAndColor(texture, color);
                UpdateTextureListeners();
            }

            private void OnTextureChanged()
            {
                Texture.OnTextureLoaded -= OnTextureChanged;
            }

            private void UpdateTextureListeners()
            {
                Texture.OnTextureLoaded -= OnTextureChanged;
                Texture.OnTextureLoaded += OnTextureChanged;
            }
        }
    }
}
