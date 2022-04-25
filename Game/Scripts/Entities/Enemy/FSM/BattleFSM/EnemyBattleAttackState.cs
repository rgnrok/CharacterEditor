using EnemySystem;

public class EnemyBattleAttackState : EnemyBattleBaseState<IBattleEntity>
{
    private EnemyAttackManager _attackManager;
    private IBattleEntity _targetEntity;

    public EnemyBattleAttackState(EnemyBattleFSM fsm) : base(fsm)
    {
    }


    public new void Enter(IBattleEntity targetEntity)
    {
        base.Enter(targetEntity);
        _attackManager = _enemy.AttackManager;
        _attackManager.OnAttackComplete += OnAttackCompleteHandler;
        _targetEntity = targetEntity;

        TryAttack(_targetEntity);
    }

    public new void Exit()
    {
        base.Exit();
        if (_attackManager != null) _attackManager.OnAttackComplete -= OnAttackCompleteHandler;
    }

    private void TryAttack(IBattleEntity battleEntity)
    {
        if (!_attackManager.IsAvailableDistance(battleEntity))
        {
            _fsm.SpawnEvent((int)EnemyBattleFSM.EnemyBattleStateType.FindTarget);

            return;
        }

        _attackManager.Attack(battleEntity);

    }

    private void OnAttackCompleteHandler()
    {
        _fsm.SpawnEvent((int)EnemyBattleFSM.EnemyBattleStateType.FindTarget);
    }
}
