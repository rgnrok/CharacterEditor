using System.Threading.Tasks;
using UnityEngine;

namespace CharacterEditor
{
    public interface ICursorLoader : ICleanable
    {
        Task<Texture2D> LoadCursor(CursorType type);
    }
}