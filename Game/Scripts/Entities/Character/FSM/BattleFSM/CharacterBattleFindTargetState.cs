using CharacterEditor;
using CharacterEditor.Services;
using UnityEngine;

public class CharacterBattleFindTargetState : CharacterBattleBaseState
{
    private readonly IInputService _inputService;
    private readonly ICharacterManageService _characterManageService;
    private readonly ICharacterRenderPathService _renderPathService;

    private CharacterAttackComponent _attackComponent;
    private IAttacked _selectedTarget;
    private ICharacterPathCalculationStrategy _pathCalculationStrategy;

    private GameManager _gameManager;

    public CharacterBattleFindTargetState(CharacterBattleFSM fsm, IInputService inputService, ICharacterManageService characterManageService, ICharacterRenderPathService renderPathService) : base(fsm)
    {
        _inputService = inputService;
        _characterManageService = characterManageService;
        _renderPathService = renderPathService;
    }

    public override void Enter()
    {
        base.Enter();

        if (CheckTurnEnd()) return;

        if (_pathCalculationStrategy == null)
            _pathCalculationStrategy = _character.EntityGameObject.GetComponent<ICharacterPathCalculationStrategy>();

        _gameManager = GameManager.Instance;
        _attackComponent = _character.AttackComponent;

        _inputService.GroundClick += OnGroundClickHandler;
        _inputService.SpacePress += OnSpacePressHandler;

        _gameManager.OnEnemyClick += OnEnemyClickHandler;

        if (!TryAttackSelectedTarget())
            _renderPathService.FireStartDrawPath(_character);
    }

    public override void Exit()
    {
        base.Exit();

        _renderPathService?.FireStartDrawPath(null);

        if (_gameManager != null)
            _gameManager.OnEnemyClick -= OnEnemyClickHandler;

        if (_inputService != null)
        {
            _inputService.GroundClick -= OnGroundClickHandler;
            _inputService.SpacePress -= OnSpacePressHandler;
        }
    }

    private bool CheckTurnEnd()
    {
        if (_character.ActionPoints.StatCurrentValue == 0 || _fsm.Enemies.Count == 0)
        {
            TurnEnd();
            return true;
        }

        return false;
    }

    private bool TryAttackSelectedTarget()
    {
        if (_selectedTarget == null) return false;

        Attack(_selectedTarget);
        return true;
    }

    private void TurnEnd()
    {
        _fsm.SpawnEvent((int)CharacterBattleFSM.CharacterBattleStateType.TurnEnd);
        _selectedTarget = null;
    }

    private void Attack(IAttacked target)
    {
        _selectedTarget = target;
        if (_attackComponent.IsAvailableDistance(target))
        {
            _fsm.SpawnEvent((int)CharacterBattleFSM.CharacterBattleStateType.Attack, target);
            _character.ActionPoints.StatCurrentValue--; //todo
            _selectedTarget = null;
            return;
        }

        var targetPoint = _attackComponent.GetTargetPointForAttack(target);
        _fsm.SpawnEvent((int)CharacterBattleFSM.CharacterBattleStateType.Move, targetPoint);
    }

    private void OnEnemyClickHandler(string characterGuid, IAttacked attacked)
    {
        if (_character == null || _character.Guid != characterGuid) return;

        Attack(attacked);
    }

    private void OnGroundClickHandler(Vector3 point)
    {
        if (_character != _characterManageService.CurrentCharacter) return;

        var distance = _pathCalculationStrategy.PathDistance(point);
        
        var actionPoints = _character.CalculateAP(distance);
        if (actionPoints > _character.ActionPoints.StatCurrentValue) return;

        _fsm.SpawnEvent((int)CharacterBattleFSM.CharacterBattleStateType.Move, point);
        _character.ActionPoints.StatCurrentValue -= actionPoints;
    }

    private void OnSpacePressHandler()
    {
        TurnEnd();
    }
}
