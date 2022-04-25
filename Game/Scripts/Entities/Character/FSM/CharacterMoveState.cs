using System;
using CharacterEditor;
using UnityEngine;

public class CharacterMoveState : CharacterBaseStateT<Vector3>
{
    private PlayerMoveComponent _moveComponent;

    private Vector3 _targetEntity;
    private bool _isExit;

    public CharacterMoveState(CharacterFSM fsm) : base(fsm)
    {
        _moveComponent = _character.MoveComponent;
    }

    public new void Enter(Vector3 targetEntity)
    {
        base.Enter(targetEntity);
        _targetEntity = targetEntity;
        _isExit = false;
        GameManager.Instance.PlayerMoveController.OnGroundClick += OnGroundClickHandler;
    }

    public new void Exit()
    {
        base.Exit();
        _isExit = true;

        if (_moveComponent != null)
        {
            _moveComponent.Stop(true);
            _moveComponent.OnMoveCompleted -= OnMoveCompletedHandler;
        }
        GameManager.Instance.PlayerMoveController.OnGroundClick -= OnGroundClickHandler;

    }

    private void AfterSwitching()
    {
        Move(_targetEntity);
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
        if (!_isExit) _fsm.SpawnEvent((int)CharacterFSM.CharacterStateType.Idle);
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
