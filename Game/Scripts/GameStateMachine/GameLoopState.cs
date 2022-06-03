using CharacterEditor.Services;

public class GameLoopState : IUpdatableState
{
    private readonly IInputService _inputService;
    private readonly IGameFactory _gameFactory;
    private readonly ICharacterEquipItemService _equipItemService;

    public GameLoopState(FSM fsm, IInputService inputService, IGameFactory gameFactory, ICharacterEquipItemService equipItemService)
    {
        _inputService = inputService;
        _gameFactory = gameFactory;
        _equipItemService = equipItemService;
    }

    public void Exit()
    {
        _gameFactory.CleanUp();
        _equipItemService.CleanUp();
    }

    public void Enter()
    {
    }

    public void Update()
    {
        _inputService.Update();
    }
}
