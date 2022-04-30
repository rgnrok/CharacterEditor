using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CharacterEditor
{
    public interface ICommonLoader<T> : IService where T : UnityEngine.Object
    {
        void LoadByPath(string path, Action<string, T> callback);
        void LoadByPath(List<string> paths, Action<Dictionary<string, T>> callback);

        void Unload(string path);
        Task<T> LoadByPath(string path);
    }
}