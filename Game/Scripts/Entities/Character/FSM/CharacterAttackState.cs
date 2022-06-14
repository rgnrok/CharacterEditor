using CharacterEditor;
using CharacterEditor.FmsPayload;
using CharacterEditor.Services;

public class CharacterAttackState : IPayloadedState<IAttacked>
{
    private readonly CharacterFSM _fsm;
    private readonly ICharacterMoveService _characterMoveService;
    private readonly CharacterAttackComponent _attackComponent;
    private readonly Character _character;

    private IAttacked _targetEntity;

    public CharacterAttackState(CharacterFSM fsm, ICharacterMoveService characterMoveService)
    {
        _fsm = fsm;
        _characterMoveService = characterMoveService;
        _character = fsm.Character;
        _attackComponent = _character.AttackComponent;
    }

    public void Enter(IAttacked targetEntity)
    {
        _targetEntity = targetEntity;
        if (_character.MoveComponent != null)
            _character.MoveComponent.OnMoveCompleted += OnMoveCompletedHandler;

        _characterMoveService.FireShowAttackPoint(_character.Guid, _targetEntity.EntityGameObject.transform.position);
        Attack();
    }

    public void Exit()
    {
        if (_character.MoveComponent != null)
            _character.MoveComponent.OnMoveCompleted -= OnMoveCompletedHandler;
    }

  

    private void Attack()
    {
        if (_attackComponent == null) return;

        if (!_attackComponent.IsAvailableDistance(_targetEntity))
        {
            var movePayload = new MovePayload(_attackComponent.GetTargetPointForAttack(_targetEntity));
            _fsm.SpawnEvent((int)CharacterFSM.CharacterStateType.Move, movePayload);
            return;
        }

        _attackComponent.Attack(_targetEntity);
    }

    private void OnMoveCompletedHandler()
    {
        _characterMoveService.FireHideCharacterPointer(_character.Guid);

        if (_attackComponent.IsAvailableDistance(_targetEntity))
            _fsm.SpawnEvent((int) CharacterFSM.CharacterStateType.Attack, _targetEntity);
    }
}
