using EnemySystem;

public class EnemyBattleAttackState : EnemyBattleBaseState<IBattleEntity>
{
    private EnemyAttackComponent _attackComponent;
    private IBattleEntity _targetEntity;

    public EnemyBattleAttackState(EnemyBattleFSM fsm) : base(fsm)
    {
    }


    public new void Enter(IBattleEntity targetEntity)
    {
        base.Enter(targetEntity);
        _attackComponent = _enemy.AttackComponent;
        _attackComponent.OnAttackComplete += OnAttackCompleteHandler;
        _targetEntity = targetEntity;

        TryAttack(_targetEntity);
    }

    public new void Exit()
    {
        base.Exit();
        if (_attackComponent != null) _attackComponent.OnAttackComplete -= OnAttackCompleteHandler;
    }

    private void TryAttack(IBattleEntity battleEntity)
    {
        if (!_attackComponent.IsAvailableDistance(battleEntity))
        {
            _fsm.SpawnEvent((int)EnemyBattleFSM.EnemyBattleStateType.FindTarget);

            return;
        }

        _attackComponent.Attack(battleEntity);

    }

    private void OnAttackCompleteHandler()
    {
        _fsm.SpawnEvent((int)EnemyBattleFSM.EnemyBattleStateType.FindTarget);
    }
}
