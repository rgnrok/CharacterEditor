using System;
using CharacterEditor;
using UnityEngine;

public class CharacterAttackState : CharacterBaseStateT<IAttacked>
{
    private IAttacked _targetEntity;
    private CharacterAttackManager _attackManager;

    public CharacterAttackState(CharacterFSM fsm) : base(fsm)
    {
        _attackManager = _character.AttackManager;
    }

    public new void Enter(IAttacked targetEntity)
    {
        base.Enter(targetEntity);

        _targetEntity = targetEntity;
        if (_character.MoveComponent != null)
            _character.MoveComponent.OnMoveCompleted += MoveComponentHandler;

        AfterSwitching();
    }

    public new void Exit()
    {
        base.Exit();
        if (_character.MoveComponent != null)
            _character.MoveComponent.OnMoveCompleted -= MoveComponentHandler;
    }

    private void AfterSwitching()
    {
        Attack();

        GameManager.Instance.PlayerMoveController.ShowCharacterPointer(_character, _targetEntity.EntityGameObject.transform.position, true);
    }
  

    private void Attack()
    {
        if (_attackManager == null) return;

        if (!_attackManager.IsAvailableDistance(_targetEntity))
        {
            _fsm.SpawnEvent((int)CharacterFSM.CharacterStateType.Move, _attackManager.GetTargetPointForAttack(_targetEntity));

            return;
        }

        _attackManager.Attack(_targetEntity);
    }

    private void MoveComponentHandler()
    {
        GameManager.Instance.PlayerMoveController.HideCharacterPointer(_character);
        if (_attackManager.IsAvailableDistance(_targetEntity))
        {
            _fsm.SpawnEvent((int)CharacterFSM.CharacterStateType.Attack, _targetEntity);
        }
    }
}
