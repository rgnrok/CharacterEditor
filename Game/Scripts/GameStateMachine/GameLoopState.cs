using CharacterEditor;
using CharacterEditor.Services;
using Game;

public class GameLoopState : IUpdatableState
{
    private readonly IInputService _inputService;
    private readonly IGameFactory _gameFactory;
    private readonly ICharacterEquipItemService _equipItemService;
    private readonly IBattleManageService _battleManager;

    public GameLoopState(FSM fsm, IInputService inputService, IGameFactory gameFactory,
        ICharacterEquipItemService equipItemService, IBattleManageService battleManager)
    {
        _inputService = inputService;
        _gameFactory = gameFactory;
        _equipItemService = equipItemService;
        _battleManager = battleManager;
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
        _battleManager.Update();
    }
}
