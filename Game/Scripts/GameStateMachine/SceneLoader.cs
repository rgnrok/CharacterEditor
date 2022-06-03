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

        public void Load(string name, Action onLoaded = null)
        {
            _coroutineRunner.StartCoroutine(LoadScene(name, onLoaded));
        }

        private IEnumerator LoadScene(string name, Action onLoaded = null)
        {
            if (SceneManager.GetActiveScene().name != name)
                yield return UnloadCurrentScene();

            var waitNextScene = SceneManager.LoadSceneAsync(name, LoadSceneMode.Single);
            while (!waitNextScene.isDone)
                yield return null;

            onLoaded?.Invoke();
        }

        private IEnumerator UnloadCurrentScene()
        {
            yield return SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene().name);
        }
    }
}