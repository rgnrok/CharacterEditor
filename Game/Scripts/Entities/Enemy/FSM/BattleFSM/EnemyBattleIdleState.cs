public class EnemyBattleIdleState : EnemyBattleBaseState
{
    public EnemyBattleIdleState(EnemyBattleFSM fsm) : base(fsm)
    {
    }

    public new void Enter()
    {
        base.Enter();

        if (_moveComponent != null)
            _moveComponent.Stop();

    }
}
