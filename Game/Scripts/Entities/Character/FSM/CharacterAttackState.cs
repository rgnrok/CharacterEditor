using System;
using CharacterEditor;
using UnityEngine;

public class CharacterAttackState : CharacterBasePayloadState<IAttacked>
{
    private IAttacked _targetEntity;
    private readonly CharacterAttackComponent _attackComponent;

    public CharacterAttackState(CharacterFSM fsm) : base(fsm)
    {
        _attackComponent = _character.AttackComponent;
    }

    public override void Enter(IAttacked targetEntity)
    {
        base.Enter(targetEntity);

        _targetEntity = targetEntity;
        if (_character.MoveComponent != null)
            _character.MoveComponent.OnMoveCompleted += MoveComponentHandler;

        AfterSwitching();
    }

    public override void Exit()
    {
        if (_character.MoveComponent != null)
            _character.MoveComponent.OnMoveCompleted -= MoveComponentHandler;
        base.Exit();
    }

    private void AfterSwitching()
    {
        Attack();

        GameManager.Instance.PlayerMoveController.ShowCharacterPointer(_character, _targetEntity.EntityGameObject.transform.position, true);
    }
  

    private void Attack()
    {
        if (_attackComponent == null) return;

        if (!_attackComponent.IsAvailableDistance(_targetEntity))
        {
            _fsm.SpawnEvent((int)CharacterFSM.CharacterStateType.Move, _attackComponent.GetTargetPointForAttack(_targetEntity));

            return;
        }

        _attackComponent.Attack(_targetEntity);
    }

    private void MoveComponentHandler()
    {
        GameManager.Instance.PlayerMoveController.HideCharacterPointer(_character);
        if (_attackComponent.IsAvailableDistance(_targetEntity))
        {
            _fsm.SpawnEvent((int)CharacterFSM.CharacterStateType.Attack, _targetEntity);
        }
    }
}
