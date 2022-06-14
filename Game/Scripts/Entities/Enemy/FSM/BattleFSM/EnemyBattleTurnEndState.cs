using System;

public class EnemyBattleTurnEndState : EnemyBattleBaseState
{

    public event Action OnTurnEnd;

    public EnemyBattleTurnEndState(EnemyBattleFSM fsm) : base(fsm)
    {
    }

    public override void Enter()
    {
        base.Enter();

        OnTurnEnd?.Invoke();
        _fsm.SpawnEvent((int)EnemyBattleFSM.EnemyBattleStateType.Idle);
    }
}
