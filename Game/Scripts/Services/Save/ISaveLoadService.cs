using System;
using System.Threading.Tasks;

namespace CharacterEditor
{
    public interface ISaveLoadService : IService
    {
        string[] GetSaves();
        void Save(string saveName, CharacterGameObjectData data);
        Task<bool> Load(string saveName, Action<int> loadProcessAction);
        string GetLastSave();
    }
}