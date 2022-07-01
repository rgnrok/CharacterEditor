using System.Collections;
using System.Collections.Generic;
using CharacterEditor;
using CharacterEditor.Services;
using EnemySystem;

public class EnemyBattleState : EnemyBaseState, IUpdatableState
{
    private bool _isTurnComplete;
    private EnemyAttackComponent _attackComponent;
    private EnemyBattleFSM _battleFSM;
    private readonly IBattleManageService _battleManageService;


    public EnemyBattleState(EnemyFSM fsm) : base(fsm)
    {
        _battleFSM = new EnemyBattleFSM(_enemy);

        _battleManageService = AllServices.Container.Single<IBattleManageService>();
    }

    public override void Enter()
    {
        base.Enter();

        _battleFSM.OnTurnEnd += OnTurnEndHandler;
        _battleFSM.OnCurrentStateChanged += OnCurrentStateChangedHandler;
        _battleFSM.Start();

        _battleManageService.OnBattleEnd += OnBattleEndHandler;
        _enemy.GameObjectData.Animator.StartBattle();

        if (_detectCollider != null) _detectCollider.IncreaseDetectCollider();
    }

    public override void Exit()
    {
        base.Exit();
        _battleFSM.OnCurrentStateChanged -= OnCurrentStateChangedHandler;
        _battleFSM.OnTurnEnd -= OnTurnEndHandler;
        _battleFSM.Clean();

        _battleManageService.OnBattleEnd -= OnBattleEndHandler;
        if (_enemy.IsAlive()) _enemy.GameObjectData.Animator.EndBattle();
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

    public IEnumerator StartTurn(List<IBattleEntity> characters)
    {
        if (characters.Count == 0)
        {
            _isTurnComplete = true;
            yield break;
        }
        if (_moveComponent != null) yield return _moveComponent.EnableNavmesh();


        _isTurnComplete = false;
        _battleFSM.StartTurn(characters);
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

    public override string ToString()
    {
        return $"{GetType().Name}: {_battleFSM.CurrentState}";
    }
}
