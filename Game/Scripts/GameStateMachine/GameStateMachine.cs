using CharacterEditor;
using CharacterEditor.Services;
using Game;

public class GameStateMachine : FSM
{
    public enum GameStateType
    {
        LoadProgress,

        CreateGame,
        CreateCharacterLoop,

        LoadGame,
        GameLoop,

        Battle
    }

    private readonly BootstrapState _bootstrapState;

    public GameStateMachine(SceneLoader sceneLoader, LoadingCurtain loadingCurtain, ICoroutineRunner coroutineRunner, AllServices services)
    {
        _bootstrapState = AddState(new BootstrapState(this, coroutineRunner, services));

        var loadProgressState = AddState(new LoadProgressState(this, services.Single<ISaveService>()));

        var createGameState = AddState(new CreateGameState(this, 
            sceneLoader, 
            loadingCurtain, 
            services.Single<ILoaderService>(),
            services.Single<IConfigManager>(),
            services.Single<IRegisterService>()
            ));

        var loadGameState = AddState(new LoadGameState(
            this, 
            sceneLoader, 
            loadingCurtain, 
            services.Single<ILoaderService>(),
            services.Single<ISaveLoadStorage>(),
            services.Single<IStaticDataService>(),
            services.Single<IMergeTextureService>(),
            services.Single<IRegisterService>()
            ));

        var createCharacterLoopState = AddState(new CreateCharacterLoopState(this));
        var gameLoopState = AddState(new GameLoopState(this, services.Single<IInputService>()));
        var battleState = AddState(new BattleState(this));

        AddTransition((int) GameStateType.LoadProgress, _bootstrapState, loadProgressState);

        AddTransition((int) GameStateType.CreateGame, loadProgressState, createGameState);
        AddTransition((int) GameStateType.CreateGame, loadGameState, createGameState);
        AddTransition((int) GameStateType.CreateGame, gameLoopState, createGameState);

        AddTransition((int) GameStateType.LoadGame, loadProgressState, loadGameState);
        AddTransition((int) GameStateType.LoadGame, createGameState, loadGameState);
        AddTransition((int) GameStateType.LoadGame, createCharacterLoopState, loadGameState);
        AddTransition((int) GameStateType.LoadGame, gameLoopState, loadGameState);

        AddTransition((int) GameStateType.CreateCharacterLoop, createGameState, createCharacterLoopState);
        AddTransition((int) GameStateType.GameLoop, loadGameState, gameLoopState);
        AddTransition((int) GameStateType.Battle, gameLoopState, battleState);
    }

    public override void Start()
    {
        Switch(_bootstrapState);
    }
}
