﻿public class EnemyDeadState : EnemyBaseState
{
    public EnemyDeadState(EnemyFSM fsm) : base(fsm)
    {
    }

    public override void Enter()
    {
        Die();
    }

    private void Die()
    {
        if (_enemy == null) return;

        _enemy.GameObjectData.Animator.SetTrigger(Constants.CHARACTER_DIE_TRIGGER);
    }

}
