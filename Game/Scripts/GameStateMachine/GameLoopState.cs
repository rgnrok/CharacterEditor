using CharacterEditor.Services;

public class GameLoopState : IUpdatableState
{
    private readonly IInputService _inputService;

    public GameLoopState(FSM fsm, IInputService inputService)
    {
        _inputService = inputService;
    }

    public void Exit()
    {
        
    }

    public void Enter()
    {
    }

    public void Update()
    {
        _inputService.Update();
    }
}
