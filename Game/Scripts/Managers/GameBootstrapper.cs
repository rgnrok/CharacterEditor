using CharacterEditor;
using Game;
using UnityEngine;

public class GameBootstrapper : MonoBehaviour, ICoroutineRunner
{
    [SerializeField]
    private LoadingCurtain _loadingCurtain;

    private GameStateMachine _gameStateMachine;

    private void Awake()
    {
        DontDestroyOnLoad(this);
        _gameStateMachine = new GameStateMachine(new SceneLoader(this), Instantiate(_loadingCurtain), this, AllServices.Container);
    }

    private void Start()
    {
        _gameStateMachine.Start();
    }

    private void Update()
    {
        _gameStateMachine.Update();
    }
}
