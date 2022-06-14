 using System;
 using UnityEngine;

public abstract class AttackComponent : MonoBehaviour
{
    [SerializeField]
    private Animator _animator;
    protected Animator Animator
    {
        get
        {
            if (_animator == null) _animator = GetComponentInChildren<Animator>();
            return _animator;
        }
    }

    private PlayerMoveComponent _moveComponent;
    protected PlayerMoveComponent MoveComponent
    {
        get
        {
            if (_moveComponent == null) _moveComponent = GetComponent<PlayerMoveComponent>();
            return _moveComponent;
        }
    }

    protected abstract float AttackDistance { get; }

    public bool IsAvailableDistance(float distance)
    {
        return distance <= AttackDistance;
    }


    public bool IsAvailableDistance(Vector3 point)
    {
        return Helper.IsNear(transform.position, point, AttackDistance); 
    }

    public virtual Vector3 GetTargetPointForAttack(Vector3 target)
    {
        var direction = Helper.GetDirection(transform.position, target);
        return target - direction * (AttackDistance - 0.1f);
    }

    public abstract void Attack(IAttacked enity, Action completeHandler);

}