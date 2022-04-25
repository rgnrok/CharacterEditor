using System;
using EnemySystem;

public class EnemyBattleFSM : FSM
{
    public enum EnemyBattleStateType
    {
        Idle,
        FindTarget,
        Move,
        Attack,
        TurnEnd,
    }

    private readonly EnemyBattleIdleState _idleState;
    private EnemyBattleMoveState _moveState;
    private EnemyBattleAttackState _attackState;
    private EnemyBattleTurnEndState _turnEndState;

    public event Action OnTurnEnd;


    public Enemy Enemy { get; private set; }

    public EnemyBattleFSM(Enemy enemy)
    {
        Enemy = enemy;

        _idleState = AddState(new EnemyBattleIdleState(this));
        var findTargetState = AddState(new EnemyBattleFindTargetState(this));
        _turnEndState = AddState(new EnemyBattleTurnEndState(this));
        _moveState = AddState(new EnemyBattleMoveState(this));
        _attackState = AddState(new EnemyBattleAttackState(this));

        AddTransition((int)EnemyBattleStateType.FindTarget, _idleState, findTargetState);
        AddTransition((int)EnemyBattleStateType.FindTarget, _moveState, findTargetState);
        AddTransition((int)EnemyBattleStateType.FindTarget, _attackState, findTargetState);

        AddTransition((int)EnemyBattleStateType.Move, findTargetState, _moveState);
        AddTransition((int)EnemyBattleStateType.Attack, findTargetState, _attackState);

        AddTransition((int)EnemyBattleStateType.Idle, _turnEndState, _idleState);

        AddGlobalTransition((int)EnemyBattleStateType.TurnEnd, _turnEndState);

        _turnEndState.OnTurnEnd += OnTurnEndHandler;
    }

    public override void Start()
    {
        Switch(_idleState);
    }

    private void OnTurnEndHandler()
    {
        if (OnTurnEnd != null) OnTurnEnd();
    }

    public void Clean()
    {
    }
}
