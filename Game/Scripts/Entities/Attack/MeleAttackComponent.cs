using System;
using CharacterEditor;
using UnityEngine;
using Random = UnityEngine.Random;

public class MeleAttackComponent : AttackComponent
{
    public override float AttackDistance => 1.3f;

    private IAttacked _entity;
    private Action _completeHandler;

    public override void Attack(IAttacked entity, Action completeHandler)
    {
        _animatorEventReceiver.OnAttack -= OnAttackHandler;

        _entity = entity;
        _completeHandler = completeHandler;

        _moveComponent.RotateTo(entity.EntityGameObject.transform.position);
        _animatorEventReceiver.OnAttack += OnAttackHandler;

        _characterAnimator.Attack();
    }

    private void OnAttackHandler()
    {
        _animatorEventReceiver.OnAttack -= OnAttackHandler;

        var dmg = (_entity is Character) ? 1 : Random.Range(1, 2);//todo
        _entity.Health.StatCurrentValue -= dmg;
        _completeHandler?.Invoke();
    }

    public override Vector3 GetTargetPointForAttack(GameObject target)
    {
        if (_moveComponent == null || _pathCalculationStrategy == null) return base.GetTargetPointForAttack(target);

        var targetPoint = _pathCalculationStrategy.GetAttackPoint(this, target);
        return targetPoint ;
    }
}
