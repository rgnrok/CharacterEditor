public class CharacterBattleIdleState : CharacterBattleBaseState
{
    public CharacterBattleIdleState(CharacterBattleFSM fsm) : base(fsm)
    {
    }

    public new void Enter()
    {
        base.Enter();

        if (_moveComponent != null)
            _moveComponent.Stop();

    }
}
