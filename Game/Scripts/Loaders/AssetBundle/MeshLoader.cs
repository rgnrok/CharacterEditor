using System.Threading.Tasks;
using Game;
using UnityEngine;

namespace CharacterEditor
{
    namespace AssetBundleLoader
    {
        public class MeshLoader : CommonLoader<GameObject>, IMeshLoader
        {
            public MeshLoader(ICoroutineRunner coroutineRunner) : base(coroutineRunner)
            {
            }
        }
    }
}