﻿using System;
using System.Collections;
using System.Collections.Generic;
using CharacterEditor;
using UnityEngine;

public class CharacterBattleFindTargetPayloadState : CharacterBattleBasePayloadState<List<IBattleEntity>>
{
    private List<IBattleEntity> _entities;
    private CharacterAttackComponent _attackComponent;
    private IAttacked _selectedTarget;
    private InputManager _inputManager;


    public CharacterBattleFindTargetPayloadState(CharacterBattleFSM fsm) : base(fsm)
    {
    }

    public override void Exit()
    {
        base.Exit();
        GameManager.Instance.RenderPathController.SetCharacter(null);
        GameManager.Instance.OnEnemyClick -= OnEnemyClickHandler;
        GameManager.Instance.PlayerMoveController.OnGroundClick -= OnGroundClickHandler;

        if (_inputManager != null)
            _inputManager.SpacePress -= OnSpacePressHandler;
    }



    public override void Enter(List<IBattleEntity> entities)
    {
        base.Enter(entities);

        _entities = entities;
        _attackComponent = _character.AttackComponent;

        GameManager.Instance.OnEnemyClick += OnEnemyClickHandler;
        GameManager.Instance.PlayerMoveController.OnGroundClick += OnGroundClickHandler;

        _inputManager = AllServices.Container.Single<InputManager>();
        _inputManager.SpacePress += OnSpacePressHandler;


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

    private void OnGroundClickHandler(string characterGuid, Vector3 point)
    {
        if (_character == null || _character.Guid != characterGuid) return;

        _fsm.SpawnEvent((int)CharacterBattleFSM.CharacterBattleStateType.Move, point);
        _character.ActionPoints.StatCurrentValue--; //todo
    }

    private void OnSpacePressHandler()
    {
        TurnEnd();
    }
}
