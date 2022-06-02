using System;
using CharacterEditor;
using CharacterEditor.FmsPayload;
using CharacterEditor.Services;
using UnityEditor;
using UnityEngine;

public class CharacterMoveState : IPayloadedState<MovePayload>
{
    private readonly CharacterFSM _fsm;
    private readonly IInputService _inputService;
    private readonly ICharacterMoveService _moveService;
    private readonly ICharacterManageService _characterManageService;
    private readonly PlayerMoveComponent _moveComponent;

    private MovePayload _target;
    private readonly Character _character;

    public CharacterMoveState(CharacterFSM fsm, IInputService inputService, ICharacterMoveService moveService, ICharacterManageService characterManageService)
    {
        _fsm = fsm;
        _inputService = inputService;
        _moveService = moveService;
        _characterManageService = characterManageService;

        _character = _fsm.Character;
        _moveComponent = _character.MoveComponent;
    }

    public void Enter(MovePayload target)
    {
        _target = target;

        _inputService.GroundClick += GroundClickHandler;
        GameManager.Instance.OnEnemyClick += OnEnemyClickHandler; //todo

        Move(_target.Point);
    }

    public void Exit()
    {
        _moveComponent.OnMoveCompleted -= OnMoveCompletedHandler;
        _moveComponent.Stop(true);

        _inputService.GroundClick -= GroundClickHandler;
        GameManager.Instance.OnEnemyClick -= OnEnemyClickHandler;
    }

    private void Move(Vector3 point)
    {
        _moveService.FireShowMovePoint(_character.Guid, point);
        _moveComponent.OnMoveCompleted -= OnMoveCompletedHandler;

        _moveComponent.OnMoveCompleted += OnMoveCompletedHandler;
        _moveComponent.MoveToPoint(point, true);
    }

    private void OnMoveCompletedHandler()
    {
        _moveComponent.OnMoveCompleted -= OnMoveCompletedHandler;

        _moveService.FireHideCharacterPointer(_character.Guid);
        _fsm.SpawnEvent((int)CharacterFSM.CharacterStateType.Idle);
        if (_target != null && _target.Callback != null && Helper.IsNear(_moveComponent.transform.position, _target.Point))
        {
            _target.Callback.Invoke();
            return;
        }

        // _fsm.SpawnEvent((int)CharacterFSM.CharacterStateType.Idle);
    }

    private void OnEnemyClickHandler(string characterGuid, IAttacked attacked)
    {
        if (_characterManageService.CurrentCharacter?.Guid != characterGuid) return;
        _fsm.SpawnEvent((int)CharacterFSM.CharacterStateType.Attack, attacked);
    }

    private void GroundClickHandler(Vector3 point)
    {
        if (_characterManageService.CurrentCharacter != _character) return;
        _target = null;
        Move(point);
    }
}
