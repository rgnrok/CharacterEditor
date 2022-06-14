using EnemySystem;

public class EnemyBattleBaseState : IState
{
    protected readonly EnemyBattleFSM _fsm;
    protected Enemy _enemy;
    protected PlayerMoveComponent _moveComponent;

    public EnemyBattleBaseState(EnemyBattleFSM fsm)
    {
        _fsm = fsm;
        _enemy = fsm.Enemy;
    }

    public virtual void Enter()
    {
        _moveComponent = _enemy.GameObjectData.Entity.GetComponent<PlayerMoveComponent>();
    }

    public virtual void Exit()
    {
        
    }
}
