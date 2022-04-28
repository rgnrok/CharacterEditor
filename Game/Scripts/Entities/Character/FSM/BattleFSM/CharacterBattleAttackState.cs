using CharacterEditor;

public class CharacterBattleAttackPayloadState : CharacterBattleBasePayloadState<IAttacked>
{
    private CharacterAttackManager _attackManager;
    private IAttacked _targetEntity;

    public CharacterBattleAttackPayloadState(CharacterBattleFSM fsm) : base(fsm)
    {
    }

    public override void Enter(IAttacked targetEntity)
    {
        base.Enter(targetEntity);
        _attackManager = _character.AttackManager;
        _attackManager.OnAttackComplete += OnAttackCompleteHandler;
        _targetEntity = targetEntity;

        TryAttack(_targetEntity);
    }

    public override void Exit()
    {
        if (_attackManager != null) _attackManager.OnAttackComplete -= OnAttackCompleteHandler;
        base.Exit();
    }


    private void TryAttack(IAttacked battleEntity)
    {
        if (!_attackManager.IsAvailableDistance(battleEntity))
        {
            _fsm.SpawnEvent((int)CharacterBattleFSM.CharacterBattleStateType.FindTarget);
            return;
        }

        _attackManager.Attack(battleEntity);

    }

    private void OnAttackCompleteHandler()
    {
        _fsm.SpawnEvent((int)CharacterBattleFSM.CharacterBattleStateType.FindTarget);
    }
}
