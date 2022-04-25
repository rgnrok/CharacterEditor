using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace CharacterEditor.Services
{
    public interface IAssets : IService
    {
        GameObject Instantiate(string path);
        GameObject Instantiate(GameObject prefab);
        Task<GameObject> InstantiateAsync(string prefabPath);
        Task<GameObject> InstantiateAsync(string prefabPath, Vector3 at);
        Task<T> Load<T>(AssetReference assetReference) where T : class;
        Task<T> Load<T>(string assetPath) where T : class;
        void CleanUp();
        void InitializeAsync();
    }
}