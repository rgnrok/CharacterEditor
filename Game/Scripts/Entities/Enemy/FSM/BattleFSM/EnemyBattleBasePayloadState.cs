using CharacterEditor;
using EnemySystem;

public class EnemyBattleBasePayloadState<T> : IPayloadedState<T>
{
    protected readonly EnemyBattleFSM _fsm;
    protected Enemy _enemy;
    protected PlayerMoveComponent _moveComponent;

    public EnemyBattleBasePayloadState(EnemyBattleFSM fsm)
    {
        _fsm = fsm;
        _enemy = fsm.Enemy;
    }

    public virtual void Enter(T param)
    {
        _moveComponent = _enemy.GameObjectData.Entity.GetComponent<PlayerMoveComponent>();
    }

    public virtual void Exit()
    {
        
    }
}
