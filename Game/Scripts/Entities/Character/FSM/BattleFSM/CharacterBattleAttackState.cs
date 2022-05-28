using CharacterEditor;

public class CharacterBattleAttackPayloadState : CharacterBattleBasePayloadState<IAttacked>
{
    private CharacterAttackComponent _attackComponent;
    private IAttacked _targetEntity;

    public CharacterBattleAttackPayloadState(CharacterBattleFSM fsm) : base(fsm)
    {
    }

    public override void Enter(IAttacked targetEntity)
    {
        base.Enter(targetEntity);
        _attackComponent = _character.AttackComponent;
        _attackComponent.OnAttackComplete += OnAttackCompleteHandler;
        _targetEntity = targetEntity;

        TryAttack(_targetEntity);
    }

    public override void Exit()
    {
        if (_attackComponent != null) _attackComponent.OnAttackComplete -= OnAttackCompleteHandler;
        base.Exit();
    }


    private void TryAttack(IAttacked battleEntity)
    {
        if (!_attackComponent.IsAvailableDistance(battleEntity))
        {
            _fsm.SpawnEvent((int)CharacterBattleFSM.CharacterBattleStateType.FindTarget);
            return;
        }

        _attackComponent.Attack(battleEntity);

    }

    private void OnAttackCompleteHandler()
    {
        _fsm.SpawnEvent((int)CharacterBattleFSM.CharacterBattleStateType.FindTarget);
    }
}
