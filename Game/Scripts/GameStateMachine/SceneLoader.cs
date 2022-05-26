using System;
using System.Collections;
using UnityEngine.SceneManagement;

namespace Game
{
    public class SceneLoader
    {
        private readonly ICoroutineRunner _coroutineRunner;

        public SceneLoader(ICoroutineRunner coroutineRunner)
        {
            _coroutineRunner = coroutineRunner;
        }

        public void Load(string name, Action onLoaded = null) =>
            _coroutineRunner.StartCoroutine(LoadScene(name, onLoaded));

        private IEnumerator LoadScene(string name, Action onLoaded = null)
        {
            var waitNextScene = SceneManager.LoadSceneAsync(name);
            while (!waitNextScene.isDone)
                yield return null;

            onLoaded?.Invoke();
        }
    }
}