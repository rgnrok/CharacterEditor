using UnityEngine;

namespace CharacterEditor
{
    public class CharacterTexture 
    {
        public readonly TextureType Type;

        public bool IsReady { get; private set; }
        
        public Texture2D Current { get; private set; }

        private int _selectedTextureIndex;
        public int SelectedTextureIndex
        {
            get => _selectedTextureIndex;
            protected set => SetTextureAndColor(value, SelectedColorIndex);
        }

        private int _selectedColorIndex;
        public int SelectedColorIndex
        {
            get => _selectedColorIndex;
            private set => SetTextureAndColor(SelectedTextureIndex, value);
        }

        private readonly ITextureLoader _textureLoader;
        protected readonly string[][] _textures;

        private string _prevTexturePath;
        private string _lastLoadPath;

        protected CharacterTexture(ITextureLoader loader, string[][] texturePaths)
        {
            _textureLoader = loader;
            _textures = texturePaths;
            IsReady = true;
        }

        public CharacterTexture(ITextureLoader loader, string[][] texturePaths, TextureType type, bool loadOnStart = true) : this(loader, texturePaths)
        {
            Type = type;
            if (loadOnStart) LoadTexture();
        }

        public void UnloadTexture(string path = null)
        {
            path = path ?? _textures[SelectedTextureIndex][SelectedColorIndex];
            _textureLoader.Unload(path);
        }

        public virtual void MoveNext() => 
            SelectedTextureIndex++;

        public bool HasNext() => 
            SelectedTextureIndex != _textures.Length - 1;

        public virtual void MovePrev() => 
            SelectedTextureIndex--;

        public bool HasPrev() => 
            SelectedTextureIndex != 0;

        public void SetTexture(int num) => 
            SelectedTextureIndex = num;

        public void Reset() => 
            SelectedTextureIndex = 0;

        public void Shuffle(bool withColor = false)
        {
            var texture = Random.Range(0, _textures.Length);
            if (withColor)
            {
                var color = Random.Range(0, _textures[texture].Length);
                SetTextureAndColor(texture, color);
                return;
            }
            SelectedTextureIndex = texture;
        }

        public void ShuffleWithColor(int color)
        {
            var texture = Random.Range(0, _textures.Length);
            SetTextureAndColor(texture, color);
        }

        public void MoveNextColor() {
            SelectedColorIndex++;
        }

        public void MovePrevColor() {
            SelectedColorIndex--;
        }

        public void SetColor(int num)
        {
            SelectedColorIndex = num;
        }

        public void ResetColor() {
            SelectedColorIndex = 0;
        }
        
        // Set texture and color with once loading
        public void SetTextureAndColor(int textNum, int colorNum)
        {
            textNum = GetTextureNumber(textNum);
            colorNum = GetColorNumber(colorNum);

            if (Current != null && _selectedTextureIndex == textNum && _selectedColorIndex == colorNum) return;

            _prevTexturePath = Current != null ? _textures[SelectedTextureIndex][SelectedColorIndex] : null;
            _selectedTextureIndex = textNum;
            _selectedColorIndex = colorNum;
            LoadTexture();
        }
        private int GetTextureNumber(int value) =>
            Helper.GetActualIndex(value, _textures.Length);

        private int GetColorNumber(int value) =>
            Helper.GetActualIndex(value, _textures[SelectedTextureIndex].Length);

        private void LoadTexture()
        {
            IsReady = false;

            _lastLoadPath = _textures[SelectedTextureIndex][SelectedColorIndex];
            _textureLoader.LoadByPath(_lastLoadPath, LoadingTexture);
        }

        private void LoadingTexture(string path, Texture2D texture)
        {
            if (!_lastLoadPath.Equals(path)) return;

            if (path != _prevTexturePath && _prevTexturePath != null)
                UnloadTexture(_prevTexturePath);

            Current = texture;
            IsReady = true;
        }
    }
}