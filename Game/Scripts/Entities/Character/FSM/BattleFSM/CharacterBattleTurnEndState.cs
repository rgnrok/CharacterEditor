using System;

public class CharacterBattleTurnEndState : CharacterBattleBaseState
{
    public event Action OnTurnEnd;

    public CharacterBattleTurnEndState(CharacterBattleFSM fsm) : base(fsm)
    {
    }

    public override void Enter()
    {
        base.Enter();
        _character.ActionPoints.SetValueCurrentToMax();

        _fsm.SpawnEvent((int)CharacterBattleFSM.CharacterBattleStateType.Idle);
        OnTurnEnd?.Invoke();
    }
}
