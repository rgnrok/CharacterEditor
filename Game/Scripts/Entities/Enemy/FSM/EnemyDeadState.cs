using EnemySystem;

public class EnemyDeadState : IState
{
    private readonly Enemy _enemy;

    public EnemyDeadState(EnemyFSM fsm)
    {
        _enemy = fsm.Enemy;
    }

    public void Enter()
    {
        Die();
    }

    public void Exit()
    {
    }

    private void Die()
    {
        if (_enemy == null) return;

        _enemy.GameObjectData.Animator.Die();
    }
}
