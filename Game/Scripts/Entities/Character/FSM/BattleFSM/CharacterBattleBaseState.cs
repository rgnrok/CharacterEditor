using CharacterEditor;

public abstract class CharacterBattleBaseState : IState
{
    protected readonly CharacterBattleFSM _fsm;
    protected Character _character;
    protected PlayerMoveComponent _moveComponent;

    protected CharacterBattleBaseState(CharacterBattleFSM fsm)
    {
        _fsm = fsm;
        _character = fsm.Character;
    }

    public virtual void Enter()
    {
        _moveComponent = _character.GameObjectData.CharacterObject.GetComponent<PlayerMoveComponent>();
    }

    public virtual void Exit()
    {
        
    }
}
