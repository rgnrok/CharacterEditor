using System.Threading.Tasks;
using UnityEngine;

namespace CharacterEditor
{
    namespace CharacterInventory
    {
        public class ItemTexture
        {
            public readonly TextureType Type;

            public Texture2D Texture { get; private set; }

            private readonly ITextureLoader _textureLoader;
            private readonly string _texturePath;

            public ItemTexture(ITextureLoader loader, string path, TextureType type = TextureType.Undefined)
            {
                _textureLoader = loader;
                _texturePath = path;
                Type = type;
            }

            public async Task LoadTexture()
            {
                Texture = await _textureLoader.LoadByPath(_texturePath);
            }
        }
    }
}