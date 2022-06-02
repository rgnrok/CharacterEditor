using System;
using System.Collections;
using System.Collections.Generic;
using CharacterEditor;
using CharacterEditor.Services;
using UnityEngine;

public class CharacterBattleFindTargetPayloadState : CharacterBattleBasePayloadState<List<IBattleEntity>>
{
    private List<IBattleEntity> _entities;
    private CharacterAttackComponent _attackComponent;
    private IAttacked _selectedTarget;
    private IInputService _inputService;
    private readonly ICharacterManageService _characterManageService;


    public CharacterBattleFindTargetPayloadState(CharacterBattleFSM fsm, IInputService inputService, ICharacterManageService characterManageService) : base(fsm)
    {
        _inputService = inputService;
        _characterManageService = characterManageService;
    }

    public override void Enter(List<IBattleEntity> entities)
    {
        base.Enter(entities);

        _entities = entities;
        _attackComponent = _character.AttackComponent;

        GameManager.Instance.OnEnemyClick += OnEnemyClickHandler;

        _inputService.GroundClick += OnGroundClickHandler;
        _inputService.SpacePress += OnSpacePressHandler;

        AfterSwitching();
    }

    public override void Exit()
    {
        base.Exit();
        GameManager.Instance.RenderPathController.SetCharacter(null);
        GameManager.Instance.OnEnemyClick -= OnEnemyClickHandler;
        _inputService.GroundClick -= OnGroundClickHandler;

        if (_inputService != null)
            _inputService.SpacePress -= OnSpacePressHandler;
    }


    private void AfterSwitching()
    {
        GameManager.Instance.RenderPathController.SetCharacter(_character);
        CheckTurnEnd();
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

        _fsm.SpawnEvent((int)CharacterBattleFSM.CharacterBattleStateType.Move, point);
        _character.ActionPoints.StatCurrentValue--; //todo
    }

    private void OnSpacePressHandler()
    {
        TurnEnd();
    }
}
