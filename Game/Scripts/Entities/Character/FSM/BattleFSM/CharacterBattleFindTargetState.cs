using System.Collections.Generic;
using CharacterEditor;
using CharacterEditor.Services;
using UnityEngine;

public class CharacterBattleFindTargetState : CharacterBattleBaseState
{
    private CharacterAttackComponent _attackComponent;
    private IAttacked _selectedTarget;
    private readonly IInputService _inputService;
    private readonly ICharacterManageService _characterManageService;
    private readonly ICharacterRenderPathService _renderPathService;
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

        _gameManager = GameManager.Instance;

        _attackComponent = _character.AttackComponent;

        _inputService.GroundClick += OnGroundClickHandler;
        _inputService.SpacePress += OnSpacePressHandler;

        _gameManager.OnEnemyClick += OnEnemyClickHandler;
        _renderPathService.FireStartDrawPath(_character);

        CheckTurnEnd();
    }

    public override void Exit()
    {
        base.Exit();

        if (_gameManager != null)
        {
            _renderPathService.FireStartDrawPath(null);
            _gameManager.OnEnemyClick -= OnEnemyClickHandler;
        }

        if (_inputService != null)
        {
            _inputService.GroundClick -= OnGroundClickHandler;
            _inputService.SpacePress -= OnSpacePressHandler;
        }
    }

    private void CheckTurnEnd()
    {
        if (_character.ActionPoints.StatCurrentValue == 0)
        {
            TurnEnd();
            return;
        }

        if (_selectedTarget != null)
        {
            Attack(_selectedTarget);
        }
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

        _fsm.SpawnEvent((int)CharacterBattleFSM.CharacterBattleStateType.Move, _attackComponent.GetTargetPointForAttack(target));
    }

    private void OnEnemyClickHandler(string characterGuid, IAttacked attacked)
    {
        if (_character == null || _character.Guid != characterGuid) return;

        Attack(attacked);
    }

    private void OnGroundClickHandler(Vector3 point)
    {
        if (_character != _characterManageService.CurrentCharacter) return;

        var distance = Vector3.Distance(_character.EntityGameObject.transform.position, point);
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
