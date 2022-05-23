using System.Threading.Tasks;
using UnityEngine;

namespace CharacterEditor
{
    namespace AddressableLoader
    {
        public class CursorLoader : ICursorLoader
        {
            private const string CURSOR_GROUP_NAME = "cursors";

            private readonly ICommonLoader<Texture2D> _textureLoader;

            public CursorLoader(ICommonLoader<Texture2D> textureLoader)
            {
                _textureLoader = textureLoader;
            }

            public Task<Texture2D> LoadCursor(CursorType type)
            {
                var name = Helper.GetCursorTextureNameByType(type);
                if (string.IsNullOrEmpty(name)) return Task.FromResult<Texture2D>(null);

                return _textureLoader.LoadByPath(GetCursorPath(name));
            }

            public static string GetCursorPath(string name)
            {
                return $"{CURSOR_GROUP_NAME}/{name}";
            }

            public void CleanUp()
            {
                _textureLoader.CleanUp();
            }
        }
    }
}