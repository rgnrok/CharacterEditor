 using System;
 using CharacterEditor;
 using UnityEngine;

public abstract class AttackComponent : MonoBehaviour
{
    [SerializeField] protected ICharacterAnimator _characterAnimator;
    [SerializeField] protected AnimatorEventReceiver _animatorEventReceiver;
    [SerializeField] protected PlayerMoveComponent _moveComponent;
    [SerializeField] protected ICharacterPathCalculationStrategy _pathCalculationStrategy;
  

    public abstract float AttackDistance { get; }

    public abstract void Attack(IAttacked entity, Action completeHandler);

    protected virtual void Awake()
    {
        if (_characterAnimator == null) _characterAnimator = GetComponentInChildren<ICharacterAnimator>();
        if (_animatorEventReceiver == null) _animatorEventReceiver = GetComponentInChildren<AnimatorEventReceiver>();

        if (_moveComponent == null) _moveComponent = GetComponent<PlayerMoveComponent>();
        if (_pathCalculationStrategy == null) _pathCalculationStrategy = GetComponent<ICharacterPathCalculationStrategy>();
    }

    public bool IsAvailableDistance(float distance)
    {
        return distance <= AttackDistance;
    }


    public bool IsAvailableDistance(Vector3 point)
    {
        return Helper.IsNear(transform.position, point, AttackDistance); 
    }

    public virtual Vector3 GetTargetPointForAttack(GameObject target)
    {
        Debug.LogWarning("BASE GetTargetPointForAttack");
        //todo tmp
        // var direction = Helper.GetDirection(transform.position, target);
        // return target - direction * (AttackDistance - 0.1f);
        return target.transform.position;
    }
}