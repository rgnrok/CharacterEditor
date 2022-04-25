using System.Collections;
using CharacterEditor;
using UnityEngine;

namespace Game
{
    public interface ICoroutineRunner : IService
    {
        Coroutine StartCoroutine(IEnumerator coroutine);
        void StopCoroutine(Coroutine coroutine);
    }
}