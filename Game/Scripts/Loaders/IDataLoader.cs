using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CharacterEditor
{
    public interface IDataLoader<T> : IService where T : UnityEngine.Object
    {
        Task<T> LoadData(string guid);
        void LoadData(string guid, Action<T> callback);
        void LoadData(List<string> guid, Action<Dictionary<string, T>> callback);
    }
}