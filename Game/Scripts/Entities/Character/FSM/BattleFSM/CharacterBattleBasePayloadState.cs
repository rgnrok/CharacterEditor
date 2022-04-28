using CharacterEditor;

public class CharacterBattleBasePayloadState<T> : IPayloadedState<T>
{
    protected readonly CharacterBattleFSM _fsm;
    protected Character _character;
    protected PlayerMoveComponent _moveComponent;


    public CharacterBattleBasePayloadState(CharacterBattleFSM fsm)
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
