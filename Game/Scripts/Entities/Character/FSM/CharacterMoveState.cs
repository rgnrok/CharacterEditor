using System;
using CharacterEditor;
using UnityEngine;

public class CharacterMoveState : CharacterBasePayloadState<Vector3>
{
    private readonly PlayerMoveComponent _moveComponent;

    private Vector3 _targetEntity;

    public CharacterMoveState(CharacterFSM fsm) : base(fsm)
    {
        _moveComponent = _character.MoveComponent;
    }

    public override void Enter(Vector3 targetEntity)
    {
        base.Enter(targetEntity);
        _targetEntity = targetEntity;

        GameManager.Instance.PlayerMoveController.OnGroundClick += OnGroundClickHandler;

        Move(_targetEntity);
    }

    public override void Exit()
    {
        base.Exit();

        if (_moveComponent != null)
        {
            _moveComponent.OnMoveCompleted -= OnMoveCompletedHandler;
            _moveComponent.Stop(true);
        }

        GameManager.Instance.PlayerMoveController.OnGroundClick -= OnGroundClickHandler;

    }


    private void Move(Vector3 point)
    {
        if (_moveComponent == null) return;

        GameManager.Instance.PlayerMoveController.ShowCharacterPointer(_character, point);
        _moveComponent.OnMoveCompleted -= OnMoveCompletedHandler;

        _moveComponent.OnMoveCompleted += OnMoveCompletedHandler;
        _moveComponent.MoveToPoint(point, true);
    }

    private void OnMoveCompletedHandler()
    {
        if (_moveComponent != null)
        {
            _moveComponent.OnMoveCompleted -= OnMoveCompletedHandler;
        }
        GameManager.Instance.PlayerMoveController.HideCharacterPointer(_character);
        _fsm.SpawnEvent((int)CharacterFSM.CharacterStateType.Idle);
    }

    protected override void OnEnemyClick(IAttacked attacked)
    {
        _fsm.SpawnEvent((int)CharacterFSM.CharacterStateType.Attack, attacked);
    }

    private void OnGroundClickHandler(string characterGuid, Vector3 point)
    {
        if (_character == null || _character.guid != characterGuid) return;

        Move(point);
    }
}
