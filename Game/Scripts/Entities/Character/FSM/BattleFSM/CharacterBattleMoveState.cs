using UnityEngine;

public class CharacterBattleMovePayloadState : CharacterBattleBasePayloadState<Vector3>
{
    private Vector3 _targetEntity;

    public CharacterBattleMovePayloadState(CharacterBattleFSM fsm) : base(fsm)
    {
    }

    public override void Enter(Vector3 targetEntity)
    {
        base.Enter(targetEntity);
        _targetEntity = targetEntity;

        AfterSwitching();
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

    private void AfterSwitching()
    {
        if (CanMove()) Move(_targetEntity);
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
        GameManager.Instance.PlayerMoveController.HideCharacterPointer(_character.Guid);//todo ??
        _fsm.SpawnEvent((int)CharacterBattleFSM.CharacterBattleStateType.FindTarget);
    }
}
