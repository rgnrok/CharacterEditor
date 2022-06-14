using CharacterEditor.Services;
using UnityEngine;

public class CharacterBattleMoveState : CharacterBattleBasePayloadState<Vector3>
{
    private readonly ICharacterMoveService _moveService;
    private Vector3 _targetEntity;

    public CharacterBattleMoveState(CharacterBattleFSM fsm, ICharacterMoveService moveService) : base(fsm)
    {
        _moveService = moveService;
    }

    public override void Enter(Vector3 targetEntity)
    {
        base.Enter(targetEntity);
        _targetEntity = targetEntity;

        if (CanMove()) Move(_targetEntity);
    }

    public override void Exit()
    {
        if (_moveComponent != null)
        {
            _moveComponent.OnMoveCompleted -= OnMoveCompletedHandler;
            _moveComponent.Stop();
        }

        base.Exit();
    }

    private bool CanMove()
    {
        return true;
    }

    private void Move(Vector3 point)
    {
        if (_moveComponent == null) return;
        _moveComponent.OnMoveCompleted -= OnMoveCompletedHandler;


        _moveComponent.OnMoveCompleted += OnMoveCompletedHandler;
        _moveComponent.MoveToPoint(point, false);
    }

    private void OnMoveCompletedHandler()
    {
        if (_moveComponent != null)
        {
            _moveComponent.OnMoveCompleted -= OnMoveCompletedHandler;
        }

        _moveService.FireHideCharacterPointer(_character.Guid);
        _fsm.SpawnEvent((int)CharacterBattleFSM.CharacterBattleStateType.FindTarget);
    }
}
