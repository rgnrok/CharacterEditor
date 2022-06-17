using System;
using CharacterEditor;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class MeleAttackComponent : AttackComponent
{
    protected override float AttackDistance => 2f;

    private IAttacked _entity;
    private Action _completeHandler;

    public override void Attack(IAttacked entity, Action completeHandler)
    {
        _animatorEventReceiver.OnAttack -= OnAttackHandler;

        _entity = entity;
        _completeHandler = completeHandler;

        _moveComponent.RotateTo(entity.EntityGameObject.transform.position);
        _animatorEventReceiver.OnAttack += OnAttackHandler;

        _animator.SetTrigger(Constants.CHARACTER_MELEE_ATTACK_1_TRIGGER);
    }

    private void OnAttackHandler()
    {
        _animatorEventReceiver.OnAttack -= OnAttackHandler;

        var dmg = (_entity is Character) ? 50 : Random.Range(10, 20);//todo
        _entity.Health.StatCurrentValue -= dmg;
        _completeHandler?.Invoke();
    }

    public override Vector3 GetTargetPointForAttack(Vector3 target)
    {
        if (_moveComponent == null) return base.GetTargetPointForAttack(target);

        var path = new NavMeshPath();
        NavMesh.CalculatePath(_moveComponent.transform.position, target, NavMesh.AllAreas, path);
        if (path.status != NavMeshPathStatus.PathComplete || path.corners.Length < 2)
            return base.GetTargetPointForAttack(target);

        var direction = Helper.GetDirection(path.corners[path.corners.Length-2], target);
        //todo check if corner distance less that attack distance ?
        return target - direction * (AttackDistance - 0.1f);
    }
}
