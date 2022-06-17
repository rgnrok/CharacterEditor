using EnemySystem;
using UnityEngine;

public class EnemyBaseState : IState
{
    protected readonly EnemyFSM _fsm;
    protected EnemyGameObjectDetectColliderComponent _detectCollider;
    protected Enemy _enemy;
    private readonly PlayerMoveComponent _moveComponent;

    protected EnemyBaseState(EnemyFSM fsm)
    {
        _fsm = fsm;
        _enemy = fsm.Enemy;
        _detectCollider = _enemy.EntityGameObject.GetComponentInChildren<EnemyGameObjectDetectColliderComponent>();
        _moveComponent = _enemy.GameObjectData.Entity.GetComponent<PlayerMoveComponent>();
    }

    public virtual void Enter()
    {
        _detectCollider.OnCharacterVisible += OnCharacterVisibleHandler;
    }

    public virtual void Exit()
    {
        _detectCollider.OnCharacterVisible -= OnCharacterVisibleHandler;
    }

    private void OnCharacterVisibleHandler(GameObject character)
    {
        if (!_enemy.IsAlive()) return;

        _moveComponent.RotateTo(character.transform.position);
        GameManager.Instance.EnemyVisibleCharacter(_enemy, character);
    }
}
