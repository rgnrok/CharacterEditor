using CharacterEditor;

public class CharacterBattleAttackPayloadState : CharacterBattleBasePayloadState<IAttacked>
{
    private CharacterAttackManager _attackManager;
    private IAttacked _targetEntity;

    public CharacterBattleAttackPayloadState(CharacterBattleFSM fsm) : base(fsm)
    {
    }

    public new void Enter(IAttacked targetEntity)
    {
        base.Enter(targetEntity);
        _attackManager = _character.AttackManager;
        _attackManager.OnAttackComplete += OnAttackCompleteHandler;
        _targetEntity = targetEntity;
    }

    public new void Exit()
    {
        base.Exit();
        if (_attackManager != null) _attackManager.OnAttackComplete -= OnAttackCompleteHandler;
    }

    private void AfterSwitching()
    {
        TryAttack(_targetEntity);
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
