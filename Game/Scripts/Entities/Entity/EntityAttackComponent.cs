using System;
using UnityEngine;

public abstract class EntityAttackComponent
{
    protected MeleAttackComponent _meleAttackComponent;
    protected RangeAttackComponent _rangeAttackComponent;
    protected PlayerMoveComponent _moveComponent;


    protected AttackComponent _currentAttackComponent;
    protected IAttacked _currentTarget;
    protected FSM _battleFsm;


    public event Action OnAttackComplete ;

    protected abstract AttackComponent GetCurrentAttackComponent();

    public EntityAttackComponent(GameObject go)
    {
        _meleAttackComponent = go.GetComponent<MeleAttackComponent>();
        _rangeAttackComponent = go.GetComponent<RangeAttackComponent>();
        _moveComponent = go.GetComponent<PlayerMoveComponent>();
    }

    public bool IsAvailableDistance(IAttacked enity)
    {
        return IsAvailableDistance(enity.EntityGameObject.transform.position);
    }

    public bool IsAvailableDistance(IBattleEntity enity)
    {
        return IsAvailableDistance(enity.EntityGameObject.transform.position);
    }

    public bool IsAvailableDistance(Vector3 targetPoint)
    {
        var attackComponent = GetCurrentAttackComponent();
        return attackComponent.IsAvailableDistance(targetPoint);
    }

    public bool IsAvailableDistance(float distance)
    {
        var attackComponent = GetCurrentAttackComponent();
        return attackComponent.IsAvailableDistance(distance);
    }

    public Vector3 GetTargetPointForAttack(IAttacked enity)
    {
        return GetTargetPointForAttack(enity.EntityGameObject.transform.position);
    }

    public Vector3 GetTargetPointForAttack(IBattleEntity enity)
    {
        return GetTargetPointForAttack(enity.EntityGameObject.transform.position);
    }

    public Vector3 GetTargetPointForAttack(Vector3 targetPoint)
    {
        var attackComponent = GetCurrentAttackComponent();
        return attackComponent.GetTargetPointForAttack(targetPoint);
    }

    public virtual void Attack(IBattleEntity enity)
    {
        Attack((IAttacked)enity);
    }

    public virtual void Attack(IAttacked enity)
    {
        if (enity == null) return;

        _currentAttackComponent = GetCurrentAttackComponent();
        _currentTarget = enity;

        var targetPoint = _currentTarget.EntityGameObject.transform.position;
        if (!_currentAttackComponent.IsAvailableDistance(targetPoint))
        {
//            _moveComponent.MoveToPoint(targetPoint);
//            _moveComponent.OnMoveCompleted += OnMoveCompletedHandler;
            return;
        }

        _currentAttackComponent.Attack(_currentTarget, CompleteHandler);
    }

    private void OnMoveCompletedHandler()
    {
//        _moveComponent.OnMoveCompleted -= OnMoveCompletedHandler;
//        Attack(_currentTarget);
    }

    private void CompleteHandler()
    {
        OnAttackComplete?.Invoke();
    }

    public bool IsRange()
    {
        return GetCurrentAttackComponent() == _rangeAttackComponent;
    }
}
