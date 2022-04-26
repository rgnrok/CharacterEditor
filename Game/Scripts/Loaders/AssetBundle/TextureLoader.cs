using Game;
using UnityEngine;

namespace CharacterEditor
{
    namespace AssetBundleLoader
    {
        public class TextureLoader : CommonLoader<Texture2D>, ITextureLoader
        {
            public TextureLoader(ICoroutineRunner coroutineRunner) : base(coroutineRunner)
            {
            }
        }
    }
}