 using System;
 using UnityEngine;

public abstract class AttackComponent : MonoBehaviour
{
    [SerializeField] protected Animator _animator;
    [SerializeField] protected AnimatorEventReceiver _animatorEventReceiver;
    [SerializeField] protected PlayerMoveComponent _moveComponent;
  

    protected abstract float AttackDistance { get; }

    public abstract void Attack(IAttacked entity, Action completeHandler);

    protected virtual void Awake()
    {
        if (_animator == null) _animator = GetComponentInChildren<Animator>();
        if (_animatorEventReceiver == null) _animatorEventReceiver = GetComponentInChildren<AnimatorEventReceiver>();

        if (_moveComponent == null) _moveComponent = GetComponent<PlayerMoveComponent>();
    }

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
}