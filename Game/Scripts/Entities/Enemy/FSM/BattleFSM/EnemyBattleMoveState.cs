using UnityEngine;

public class EnemyBattleMoveState : EnemyBattleBaseState<Vector3>
{
    public EnemyBattleMoveState(EnemyBattleFSM fsm) : base(fsm)
    {
    }

    public new void Enter(Vector3 targetEntity)
    {
        base.Enter(targetEntity);
        if (CanMove()) Move(targetEntity);
    }

    public new void Exit()
    {
        base.Exit();
        if (_moveComponent == null) return;

        _moveComponent.Stop();
        _moveComponent.OnMoveCompleted -= OnMoveCompletedHandler;
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
        _fsm.SpawnEvent((int)EnemyBattleFSM.EnemyBattleStateType.FindTarget);
    }
}
