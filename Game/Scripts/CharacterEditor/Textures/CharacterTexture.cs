using System;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace CharacterEditor
{
    public class CharacterTexture
    {
        public readonly TextureType Type;

        public bool IsReady { get; private set; }
        
        public event Action OnTextureLoaded;

        public Texture2D Current { get; private set; }

        private int _selectedTexture;
        public int SelectedTexture
        {
            get => _selectedTexture;
            protected set => SetTextureAndColor(value, SelectedColor);
        }

        private int _selectedColor;
        public int SelectedColor
        {
            get => _selectedColor;
            private set => SetTextureAndColor(SelectedTexture, value);
        }

        private readonly ITextureLoader _textureLoader;
        protected readonly string[][] _textures;


        private string _prevTexturePath;
        private string _lastLoadPath;

        public CharacterTexture(ITextureLoader loader, string[][] texturePaths, TextureType type = TextureType.Undefined)
        {
            _textureLoader = loader;
            _textures = texturePaths;
            Type = type;

            if (type == TextureType.Undefined) IsReady = true;  //todo check
            else LoadTexture();
        }

        public string GetShaderTextureName() => 
            Helper.GetShaderTextureName(Type);

        private int GetTextureNumber(int value) => 
            Helper.GetActualIndex(value, _textures.Length);

        private int GetColorNumber(int value) => 
            Helper.GetActualIndex(value, _textures[SelectedTexture].Length);

        public void UnloadTexture(string path = null)
        {
            path = path ?? _textures[SelectedTexture][SelectedColor];
            _textureLoader.Unload(path);
        }

        private void LoadTexture()
        {
            //todo Check FOR Mesh Textures without type
            IsReady = false;

            _lastLoadPath = _textures[SelectedTexture][SelectedColor];
            _textureLoader.LoadByPath(_lastLoadPath, LoadingTexture);
        }

        private void LoadingTexture(string path, Texture2D texture)
        {
            if (_lastLoadPath.Equals(path))
            {
                if (path != _prevTexturePath && _prevTexturePath != null)
                {
                    UnloadTexture(_prevTexturePath);
                }

                Current = texture;

                IsReady = true;
                OnTextureLoaded?.Invoke();
            }
        }

        public virtual void MoveNext() {
            SelectedTexture++;
        }

        public bool HasNext() {
            return SelectedTexture != _textures.Length - 1;
        }

        public virtual void MovePrev() {
            SelectedTexture--;
        }

        public bool HasPrev() {
            return SelectedTexture != 0;
        }

        public void SetTexture(int num)
        {
            SelectedTexture = num;
        }

        public void Reset() {
            SelectedTexture = 0;
        }

        public void Shuffle(bool withColor = false)
        {
            var texture = UnityEngine.Random.Range(0, _textures.Length);
            if (withColor)
            {
                var color = UnityEngine.Random.Range(0, _textures[texture].Length);
                SetTextureAndColor(texture, color);
                return;
            }
            SelectedTexture = texture;
        }

        public void ShuffleWithColor(int color)
        {
            var texture = UnityEngine.Random.Range(0, _textures.Length);
            SetTextureAndColor(texture, color);
        }

        public void MoveNextColor() {
            SelectedColor++;
        }

        public void MovePrevColor() {
            SelectedColor--;
        }

        public void SetColor(int num)
        {
            SelectedColor = num;
        }

        public void ResetColor() {
            SelectedColor = 0;
        }
        
        // Set texture and color with once loading
        public void SetTextureAndColor(int textNum, int colorNum)
        {
            textNum = GetTextureNumber(textNum);
            colorNum = GetColorNumber(colorNum);

            if (Current != null && _selectedTexture == textNum && _selectedColor == colorNum) return;

            _prevTexturePath = Current != null ? _textures[SelectedTexture][SelectedColor] : null;
            _selectedTexture = textNum;
            _selectedColor = colorNum;
            LoadTexture();
        }

        public Color32[] GetPixels32()
        {
            Debug.LogWarning($"Get pixel _prevTexturePath: {_prevTexturePath}, _lastLoadPath: {_lastLoadPath}");
            return Current.GetPixels32();
        }
    }
}