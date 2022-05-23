using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CharacterEditor
{
    public interface IDataLoader<T> : ICleanable where T : UnityEngine.Object, IData
    {
        void LoadData(string guid, Action<T> callback);
        void LoadData(IList<string> guid, Action<Dictionary<string, T>> callback);
        Task<T> LoadData(string guid);
        Task<Dictionary<string, T>> LoadData(IList<string> guid);
    }
}