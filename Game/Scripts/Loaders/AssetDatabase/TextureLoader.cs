#if UNITY_EDITOR
using UnityEngine;

namespace CharacterEditor
{
    namespace AssetDatabaseLoader
    {
        public class TextureLoader : CommonLoader<Texture2D>, ITextureLoader
        {
        }
    }
}
#endif