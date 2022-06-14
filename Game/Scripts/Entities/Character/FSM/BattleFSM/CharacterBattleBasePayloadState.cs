using CharacterEditor;

public abstract class CharacterBattleBasePayloadState<T> : IPayloadedState<T>
{
    protected readonly CharacterBattleFSM _fsm;
    protected readonly Character _character;
    protected PlayerMoveComponent _moveComponent;


    protected CharacterBattleBasePayloadState(CharacterBattleFSM fsm)
    {
        _fsm = fsm;
        _character = fsm.Character;
    }

    public virtual void Enter(T param)
    {
        _moveComponent = _character.GameObjectData.CharacterObject.GetComponent<PlayerMoveComponent>();
    }

    public virtual void Exit()
    {
    }
}
