using System;
using System.Collections;
using System.Collections.Generic;
using CharacterEditor;
using UnityEngine;

public class CharacterBattleFindTargetPayloadState : CharacterBattleBasePayloadState<List<IBattleEntity>>
{
    private readonly CharacterBattleFSM _fsm;

    private List<IBattleEntity> _entities;
    private CharacterAttackManager _attackManager;
    private IAttacked _selectedTarget;


    public CharacterBattleFindTargetPayloadState(CharacterBattleFSM fsm) : base(fsm)
    {
        _fsm = fsm;
    }

    public new void Exit()
    {
        base.Exit();
        GameManager.Instance.RenderPathController.SetCharacter(null);
        GameManager.Instance.OnEnemyClick -= OnEnemyClickHandler;
        GameManager.Instance.InputManager.SpacePress -= OnSpacePressHandler;
        GameManager.Instance.PlayerMoveController.OnGroundClick -= OnGroundClickHandler;
    }

   

    public new void Enter(List<IBattleEntity> entities)
    {
        base.Enter(entities);

        _entities = entities;
        _attackManager = _character.AttackManager;

        GameManager.Instance.OnEnemyClick += OnEnemyClickHandler;
        GameManager.Instance.InputManager.SpacePress += OnSpacePressHandler;
        GameManager.Instance.PlayerMoveController.OnGroundClick += OnGroundClickHandler;

        AfterSwitching();
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
        if (_attackManager.IsAvailableDistance(target))
        {
            _fsm.SpawnEvent((int)CharacterBattleFSM.CharacterBattleStateType.Attack, target);
            _character.ActionPoints.StatCurrentValue--; //todo
            _selectedTarget = null;
            return;
        }

        _fsm.SpawnEvent((int)CharacterBattleFSM.CharacterBattleStateType.Move, _attackManager.GetTargetPointForAttack(target));
    }

    private void OnEnemyClickHandler(string characterGuid, IAttacked attacked)
    {
        if (_character == null || _character.guid != characterGuid) return;

        Attack(attacked);
    }

    private void OnGroundClickHandler(string characterGuid, Vector3 point)
    {
        if (_character == null || _character.guid != characterGuid) return;

        _fsm.SpawnEvent((int)CharacterBattleFSM.CharacterBattleStateType.Move, point);
        _character.ActionPoints.StatCurrentValue--; //todo
    }

    private void OnSpacePressHandler()
    {
        TurnEnd();
    }
}
