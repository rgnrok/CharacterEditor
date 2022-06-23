public class CharacterBattleIdleState : CharacterBattleBaseState
{
    public CharacterBattleIdleState(CharacterBattleFSM fsm) : base(fsm)
    {
    }

    public override void Enter()
    {
        base.Enter();

        if (_moveComponent != null)
            _moveComponent.Stop(true);

    }
}
