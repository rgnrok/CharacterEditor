public class EnemyBattleIdleState : EnemyBattleBaseState
{
    public EnemyBattleIdleState(EnemyBattleFSM fsm) : base(fsm)
    {
    }

    public override void Enter()
    {
        base.Enter();

        if (_moveComponent != null)
            _moveComponent.Stop(true);

    }
}
