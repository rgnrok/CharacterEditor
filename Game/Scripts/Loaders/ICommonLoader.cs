using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CharacterEditor
{
    public interface ICommonLoader<T> : ICleanable where T : UnityEngine.Object
    {
        void LoadByPath(string path, Action<string, T> callback);
        void LoadByPath(IList<string> paths, Action<Dictionary<string, T>> callback);

        Task<T> LoadByPath(string path);
        Task<Dictionary<string, T>> LoadByPath(IList<string> paths);

        void Unload(string path);
    }
}