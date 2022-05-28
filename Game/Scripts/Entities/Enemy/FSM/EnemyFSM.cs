using System.Collections.Generic;
using EnemySystem;

public class EnemyFSM : FSM
{
    public enum EnemyStateType
    {
        Idle,
        Move,
        Battle,
        Dead
    }

    public Enemy Enemy { get; private set; }

    private readonly EnemyIdleState _idleState;
    private EnemyBattleState _battleState;

    public EnemyFSM(Enemy enemy)
    {
        Enemy = enemy;

        _idleState = AddState(new EnemyIdleState(this));
        _battleState = AddState(new EnemyBattleState(this));
        var deadState = AddState(new EnemyDeadState(this));

        AddTransition((int)EnemyStateType.Idle, _battleState, _idleState);
        AddTransition((int)EnemyStateType.Battle, _idleState, _battleState);
        AddGlobalTransition((int) EnemyStateType.Dead, deadState);
    }

    public bool IsTurnComplete()
    {
        if (CurrentState != _battleState) return true;
    
        return _battleState.IsTurnComplete();
    }
    
    public void StartTurn(List<IBattleEntity> enemies)
    {
        if (CurrentState != _battleState) return;
        _battleState.StartTurn(enemies);
    }
    
    public void ProcessTurn()
    {
        if (CurrentState != _battleState) return;
        _battleState.ProcessTurn();
    }

    public override void Start()
    {
        Switch(_idleState);
    }
}
