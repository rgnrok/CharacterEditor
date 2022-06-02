using System;
using System.Collections.Generic;
using CharacterEditor;
using CharacterEditor.Services;

public class CharacterFSM : FSM
{
    public enum CharacterStateType
    {
        Idle,
        Move,
        Attack,
        Battle,
        Dead
    }

    private readonly CharacterIdleState _idleState;
    private readonly CharacterMoveState _moveState;
    private readonly CharacterBattleState _battleState;

    public Character Character { get; }

    public CharacterFSM(Character character)
    {
        Character = character;

        var inputService = AllServices.Container.Single<IInputService>();
        var moveService = AllServices.Container.Single<ICharacterMoveService>();
        var characterManagerService = AllServices.Container.Single<ICharacterManageService>();

        _idleState = AddState(new CharacterIdleState(this, inputService, characterManagerService));
        _moveState = AddState(new CharacterMoveState(this, inputService, moveService, characterManagerService));
        _battleState = AddState(new CharacterBattleState(this));
        var deadState = AddState(new CharacterDeadState(this));
        var attackState = AddState(new CharacterAttackState(this, moveService));

        AddTransition((int)CharacterStateType.Idle, _moveState, _idleState);
        AddTransition((int)CharacterStateType.Idle, attackState, _idleState);
        AddTransition((int)CharacterStateType.Idle, _battleState, _idleState);

        AddTransition((int)CharacterStateType.Move, _idleState, _moveState);
        AddTransition((int)CharacterStateType.Move, attackState, _moveState);
        AddTransition((int)CharacterStateType.Move, _moveState, _moveState);

        AddTransition((int)CharacterStateType.Attack, _idleState, attackState);
        AddTransition((int)CharacterStateType.Attack, _moveState, attackState);
        AddTransition((int)CharacterStateType.Attack, attackState, attackState);

        AddGlobalTransition((int)CharacterStateType.Battle, _battleState);
        AddGlobalTransition((int)CharacterStateType.Dead, deadState);
    }


    public bool IsTurnComplete()
    {
        if (CurrentState != _battleState) return true;

        return _battleState.IsTurnComplete();
    }

    public void StartTurn(List<IBattleEntity> entities)
    {
        if (CurrentState != _battleState) return;
            _battleState.StartTurn(entities);
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
