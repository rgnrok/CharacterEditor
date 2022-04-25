using CharacterEditor;
using EnemySystem;

public class EnemyBattleBaseState<T> : IPayloadedState<T>
{
    protected readonly EnemyBattleFSM _fsm;
    protected Enemy _enemy;
    protected PlayerMoveComponent _moveComponent;

    public EnemyBattleBaseState(EnemyBattleFSM fsm)
    {
        _fsm = fsm;
        _enemy = fsm.Enemy;
    }

    public void Enter(T param)
    {
        _moveComponent = _enemy.GameObjectData.Entity.GetComponent<PlayerMoveComponent>();
    }

    public void Exit()
    {
        
    }
}
