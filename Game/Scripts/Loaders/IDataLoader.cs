using System;
using System.Collections.Generic;

namespace CharacterEditor
{
    public interface IDataLoader<T> : IService where T : UnityEngine.Object
    {
        void LoadData(string guid, Action<T> callback);
        void LoadData(List<string> guid, Action<Dictionary<string, T>> callback);
    }
}