using System.Collections.Generic;
using EnemySystem;

public class EnemyBattleState : EnemyBaseState, IUpdatableState
{
    private bool _isTurnComplete;
    private EnemyAttackComponent _attackComponent;
    private EnemyBattleFSM _battleFSM;

    public EnemyBattleState(EnemyFSM fsm) : base(fsm)
    {
        _battleFSM = new EnemyBattleFSM(_enemy);
        _battleFSM.OnTurnEnd += OnTurnEndHandler;
        _battleFSM.OnCurrentStateChanged += OnCurrentStateChangedHandler;
    }

    public override void Enter()
    {
        base.Enter();
        _battleFSM.Start();

        GameManager.Instance.BattleManager.OnBattleEnd += OnBattleEndHandler;
        _enemy.GameObjectData.Animator.SetTrigger(Constants.CHARACTER_START_BATTLE_TRIGGER);

        if (_detectCollider != null) _detectCollider.IncreaseDetectCollider();
    }

    public override void Exit()
    {
        base.Exit();
        _battleFSM.OnCurrentStateChanged -= OnCurrentStateChangedHandler;
        _battleFSM.Clean();

        GameManager.Instance.BattleManager.OnBattleEnd -= OnBattleEndHandler;
        if (_enemy.IsAlive()) _enemy.GameObjectData.Animator.SetTrigger(Constants.CHARACTER_END_BATTLE_TRIGGER);
        if (_detectCollider != null) _detectCollider.DecreaseDetectCollider();
    }

    public void Update()
    {
        _battleFSM.Update();
    }

    private void OnTurnEndHandler()
    {
        _isTurnComplete = true;
    }

    public bool IsTurnComplete()
    {
        return _isTurnComplete;
    }

    public void StartTurn(List<IBattleEntity> enemies)
    {
        if (enemies.Count == 0)
        {
            _isTurnComplete = true;
            return;
        }

        _isTurnComplete = false;
        _battleFSM.SpawnEvent((int)EnemyBattleFSM.EnemyBattleStateType.FindTarget, enemies);
    }

    public void ProcessTurn()
    {

    }

    private void OnBattleEndHandler()
    {
        _fsm.SpawnEvent((int)EnemyBattleFSM.EnemyBattleStateType.Idle);
    }
    
    private void OnCurrentStateChangedHandler(IExitableState state)
    {
        _fsm.FireOnCurrentStateChanged();
    }
}
