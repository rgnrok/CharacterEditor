using EnemySystem;
using UnityEngine;

public class EnemyBaseState : IState
{
    protected readonly EnemyFSM _fsm;
    protected EnemyGameObjectDetectColliderComponent _detectCollider;
    protected Enemy _enemy;

    public EnemyBaseState(EnemyFSM fsm)
    {
        _fsm = fsm;
        _enemy = fsm.Enemy;
        _detectCollider = _enemy.EntityGameObject.GetComponentInChildren<EnemyGameObjectDetectColliderComponent>();
    }

    public void Enter()
    {
        _detectCollider.OnCharacterVisible += OnCharacterVisibleHandler;
    }

    public void Exit()
    {
        _detectCollider.OnCharacterVisible -= OnCharacterVisibleHandler;
    }

    private void OnCharacterVisibleHandler(GameObject character)
    {
        if (_enemy.IsAlive()) GameManager.Instance.EnemyVisibleCharacter(_enemy, character);
    }
}
