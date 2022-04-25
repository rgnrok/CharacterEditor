using System;

public class CharacterBattleTurnEndState : CharacterBattleBaseState
{

    public event Action OnTurnEnd;

    public CharacterBattleTurnEndState(CharacterBattleFSM fsm) : base(fsm)
    {
    }

    public new void Enter()
    {
        base.Enter();
        _character.ActionPoints.SetValueCurrentToMax();

        if (OnTurnEnd != null) OnTurnEnd();
    }

    private void AfterSwitching()
    {
        _fsm.SpawnEvent((int)CharacterBattleFSM.CharacterBattleStateType.Idle);

    }
}
