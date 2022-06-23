using System;
using System.Collections.Generic;
using CharacterEditor;
using CharacterEditor.Services;

public class CharacterBattleFSM : FSM
{
    public enum CharacterBattleStateType
    {
        Idle,
        FindTarget,
        Move,
        Attack,
        TurnEnd,
    }

    private CharacterBattleMoveState _moveState;
    private CharacterBattleAttackState _attackState;
    private CharacterBattleTurnEndState _turnEndState;
    private readonly CharacterBattleIdleState _idleState;
    public List<IBattleEntity> Enemies { get; private set; }


    public event Action OnTurnEnd;


    public Character Character { get; private set; }

    public CharacterBattleFSM(Character character)
    {
        Character = character;

        var inputService = AllServices.Container.Single<IInputService>();
        var characterManageService = AllServices.Container.Single<ICharacterManageService>();
        var characterMoveService = AllServices.Container.Single<ICharacterMoveService>();
        var renderPathService = AllServices.Container.Single<ICharacterRenderPathService>();
        var pathCalculation = AllServices.Container.Single<ICharacterPathCalculation>();

        _idleState = AddState(new CharacterBattleIdleState(this));
        var findTargetState = AddState(new CharacterBattleFindTargetState(this, inputService, characterManageService, renderPathService, pathCalculation));
        _turnEndState = AddState(new CharacterBattleTurnEndState(this));
        _moveState = AddState(new CharacterBattleMoveState(this, characterMoveService));
        _attackState = AddState(new CharacterBattleAttackState(this));

        AddTransition((int)CharacterBattleStateType.FindTarget, _idleState, findTargetState);
        AddTransition((int)CharacterBattleStateType.FindTarget, _moveState, findTargetState);
        AddTransition((int)CharacterBattleStateType.FindTarget, _attackState, findTargetState);

        AddTransition((int)CharacterBattleStateType.Move, findTargetState, _moveState);
        AddTransition((int)CharacterBattleStateType.Attack, findTargetState, _attackState);

        AddTransition((int)CharacterBattleStateType.Idle, _turnEndState, _idleState);

        AddGlobalTransition((int)CharacterBattleStateType.TurnEnd, _turnEndState);

        _turnEndState.OnTurnEnd += OnTurnEndHandler;
    }

    public override void Start()
    {
        Switch(_idleState);
    }

    private void OnTurnEndHandler()
    {
        OnTurnEnd?.Invoke();
    }

    public void Clean()
    {
    }

    public void StartTurn(List<IBattleEntity> enemies)
    {
        Enemies = enemies;
        SpawnEvent((int)CharacterBattleStateType.FindTarget);
    }
}
