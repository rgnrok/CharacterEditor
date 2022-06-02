using System;
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

    private CharacterBattleMovePayloadState _movePayloadState;
    private CharacterBattleAttackPayloadState _attackPayloadState;
    private CharacterBattleTurnEndState _turnEndState;
    private readonly CharacterBattleIdleState _idleState;

    public event Action OnTurnEnd;


    public Character Character { get; private set; }

    public CharacterBattleFSM(Character character)
    {
        Character = character;

        var inputService = AllServices.Container.Single<IInputService>();
        var characterManageService = AllServices.Container.Single<ICharacterManageService>();

        _idleState = AddState(new CharacterBattleIdleState(this));
        var findTargetState = AddState(new CharacterBattleFindTargetPayloadState(this, inputService, characterManageService));
        _turnEndState = AddState(new CharacterBattleTurnEndState(this));
        _movePayloadState = AddState(new CharacterBattleMovePayloadState(this));
        _attackPayloadState = AddState(new CharacterBattleAttackPayloadState(this));

        AddTransition((int)CharacterBattleStateType.FindTarget, _idleState, findTargetState);
        AddTransition((int)CharacterBattleStateType.FindTarget, _movePayloadState, findTargetState);
        AddTransition((int)CharacterBattleStateType.FindTarget, _attackPayloadState, findTargetState);

        AddTransition((int)CharacterBattleStateType.Move, findTargetState, _movePayloadState);
        AddTransition((int)CharacterBattleStateType.Attack, findTargetState, _attackPayloadState);

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
}
