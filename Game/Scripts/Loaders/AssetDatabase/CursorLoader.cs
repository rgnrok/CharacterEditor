using System.Threading.Tasks;
using UnityEngine;

namespace CharacterEditor
{
    namespace AssetDatabaseLoader
    {
        public class CursorLoader : ICursorLoader
        {
            private static readonly string CursorFileTemplate = $"{AssetsConstants.GameRootPath}/Cursors/{{0}}.png";

            private readonly ICommonLoader<Texture2D> _textureLoader;

            public static string GetCursorPath(string name)
            {
                return string.Format(CursorFileTemplate, name);
            }

            public CursorLoader(ICommonLoader<Texture2D> textureLoader)
            {
                _textureLoader = textureLoader;
            }

            public Task<Texture2D> LoadCursor(CursorType type)
            {
                var name = Helper.GetCursorTextureNameByType(type);
                if (string.IsNullOrEmpty(name)) return null;

                return _textureLoader.LoadByPath(GetCursorPath(name));
            }

            public void CleanUp()
            {
                _textureLoader.CleanUp();
            }
        }
    }
}