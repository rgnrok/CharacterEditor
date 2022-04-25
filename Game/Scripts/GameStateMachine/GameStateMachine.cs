using CharacterEditor;
using CharacterEditor.Services;
using Game;

public class GameStateMachine : FSM
{
    public enum GameStateType
    {
        Load,
        CreateCharacter,
        PlayLoop,
        Battle
    }

    private readonly BootstrapState _bootstrapState;

    public GameStateMachine(SceneLoader sceneLoader, LoadingCurtain loadingCurtain, ICoroutineRunner coroutineRunner, AllServices services)
    {
        _bootstrapState = AddState(new BootstrapState(this, coroutineRunner, services));
        var loadState = AddState(new LoadCreateCharacterState(this, 
            sceneLoader, 
            loadingCurtain, 
            services.Single<ILoaderService>(),
            services.Single<IGameFactory>(),
            services.Single<ConfigManager>()
            ));
        var createCharacterState = AddState(new CreateCharacterdState(this));
        var worldState = AddState(new GameWorldState(this));
        var battleState = AddState(new BattleState(this));

        AddTransition((int) GameStateType.Load, _bootstrapState, loadState);
        AddTransition((int) GameStateType.CreateCharacter, loadState, createCharacterState);
        AddTransition((int) GameStateType.PlayLoop, loadState, worldState);
        AddTransition((int) GameStateType.Battle, worldState, battleState);
    }

    public override void Start()
    {
        Switch(_bootstrapState);
    }
}
