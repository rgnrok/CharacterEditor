using CharacterEditor;
using CharacterEditor.Services;

namespace Game
{
    public abstract class LoadState : IPayloadedState<string>
    {
        protected readonly FSM _fsm;
        protected readonly ILoaderService _loaderService;
        protected readonly SceneLoader _sceneLoader;
        protected readonly LoadingCurtain _loadingCurtain;

        protected LoadState(FSM fsm, SceneLoader sceneLoader, LoadingCurtain loadingCurtain, ILoaderService loaderService)
        {
            _fsm = fsm;
            _sceneLoader = sceneLoader;
            _loadingCurtain = loadingCurtain;
            _loaderService = loaderService;
        }

        public void Enter(string sceneName)
        {
            _loadingCurtain.SetLoading(0);
            _sceneLoader.Load(sceneName, OnSceneLoaded);
        }

        public void Exit()
        {
        }

        protected abstract void OnLoaded();

        private async void OnSceneLoaded()
        {
            await _loaderService.Initialize();
            _loadingCurtain.SetLoading(10);

            OnLoaded();
        }
    }
}