using System.Threading.Tasks;
using UnityEngine;

namespace CharacterEditor
{
    namespace AssetBundleLoader
    {
        public class CursorLoader : ICursorLoader
        {
            public const string CURSOR_BUNDLE_NAME = "cursors";

            private readonly ICommonLoader<Texture2D> _textureLoader;

            public CursorLoader(ICommonLoader<Texture2D> textureLoader)
            {
                _textureLoader = textureLoader;
            }

            public Task<Texture2D> LoadCursor(CursorType type)
            {
                var name = Helper.GetCursorTextureNameByType(type);
                if (string.IsNullOrEmpty(name)) return null;

                return _textureLoader.LoadByPath($"{CURSOR_BUNDLE_NAME}/{name}");
            }

            public void CleanUp()
            {
                _textureLoader.CleanUp();
            }
        }
    }
}