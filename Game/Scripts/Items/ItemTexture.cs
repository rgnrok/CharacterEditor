using System;
using UnityEngine;

namespace CharacterEditor
{
    namespace CharacterInventory
    {
        public class ItemTexture
        {
            public readonly TextureType Type;

            public bool IsReady { get; private set; }

            public Texture2D Texture
            {
                get;
                private set;
            }

            private readonly ITextureLoader _textureLoader;
            private readonly string _texturePath;

            public Action OnTextureLoaded;

            public virtual string GetShaderTextureName()
            {
                return null;
            }

            public ItemTexture(ITextureLoader loader, string path, TextureType type = TextureType.Undefined)
            {
                _textureLoader = loader;
                _texturePath = path;
                Type = type;
            }

            public void LoadTexture()
            {
                IsReady = false;
                _textureLoader.LoadByPath(_texturePath, LoadingTexture);
            }

            public Color32[] GetPixels32()
            {
                return Texture.GetPixels32();
            }

            private void LoadingTexture(string path, Texture2D texture)
            {
                if (!path.Equals(_texturePath)) return;

                Texture = texture;
                IsReady = true;
                OnTextureLoaded?.Invoke();
            }
        }
    }
}