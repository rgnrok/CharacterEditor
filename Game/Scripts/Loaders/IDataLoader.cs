using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CharacterEditor
{
    public interface IDataLoader<T> : IService where T : UnityEngine.Object
    {
        void LoadData(string guid, Action<T> callback);
        void LoadData(List<string> guid, Action<Dictionary<string, T>> callback);
        Task<T> LoadData(string guid);
        Task<Dictionary<string, T>> LoadData(List<string> guid);
    }
}