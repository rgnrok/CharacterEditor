using System;
using System.Reflection;
using System.Threading;
using UnityEngine;

namespace CharacterEditor
{
    public class GameRunner : MonoBehaviour
    {
        public GameObject bootstrapPrefab;

        private void Awake()
        {
            var bootstrapper = FindObjectOfType<GameBootstrapper>();
            if (bootstrapper) return;
            
            Instantiate(bootstrapPrefab);
        }

        private void OnApplicationQuit()
        {
#if UNITY_EDITOR
            var constructor = SynchronizationContext.Current.GetType().GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(int) }, null);
            if (constructor != null)
            {
                var newContext = constructor.Invoke(new object[] {Thread.CurrentThread.ManagedThreadId});
                SynchronizationContext.SetSynchronizationContext(newContext as SynchronizationContext);
            }
#endif
        }
    }
}