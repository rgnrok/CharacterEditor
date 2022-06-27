using System;
using System.Collections;
using System.Collections.Generic;
using CharacterEditor;
using CharacterEditor.Services;

public class CharacterBattleState : IState, IUpdatableState
{
    private PlayerMoveComponent _moveComponent;
    private CharacterAttackComponent _attackComponent;

    private bool _isTurnComplete;

//    private IBattleEntity _currentEnemy;

    private CharacterBattleFSM _battleFSM;
    private readonly CharacterFSM _fsm;
    private readonly Character _character;
    private readonly IBattleManageService _battleManageService;

    public CharacterBattleState(CharacterFSM fsm)
    {
        _fsm = fsm;
        _character = fsm.Character;
        _moveComponent = _character.EntityGameObject.GetComponent<PlayerMoveComponent>();

        _battleFSM = new CharacterBattleFSM(_character);
        _battleManageService = AllServices.Container.Single<IBattleManageService>();
    }

    public void Enter()
    {
        _battleFSM.OnTurnEnd += OnTurnEndHandler;
        _battleFSM.OnCurrentStateChanged += OnCurrentStateChangedHandler;
        _battleFSM.Start();

        _battleManageService.OnBattleEnd += OnBattleEndHandler;
        _character.GameObjectData.Animator.SetTrigger(Constants.CHARACTER_START_BATTLE_TRIGGER);
        _character.ActionPoints.SetValueCurrentToMax();
    }

    public void Exit()
    {
        _battleFSM.OnTurnEnd -= OnTurnEndHandler;
        _battleFSM.OnCurrentStateChanged -= OnCurrentStateChangedHandler;
        _battleFSM.Clean();

        _battleManageService.OnBattleEnd -= OnBattleEndHandler;
        if (_character.IsAlive()) _character.GameObjectData.Animator.SetTrigger(Constants.CHARACTER_END_BATTLE_TRIGGER);
    }

    public void Update()
    {
        _battleFSM.Update();
    }


    private void OnTurnEndHandler()
    {
        _isTurnComplete = true;
        if (_moveComponent != null) _moveComponent.DisableNavmesh();
    }

    public bool IsTurnComplete()
    {
        return _isTurnComplete;
    }

    public IEnumerator StartTurn(List<IBattleEntity> enemies)
    {
        if (enemies.Count == 0)
        {
            _isTurnComplete = true;
            yield break;
        }

        if (_moveComponent != null)
            yield return _moveComponent.EnableNavmesh();

        _isTurnComplete = false;
        _battleFSM.StartTurn(enemies);
    }

    public void ProcessTurn()
    {

    }

    private void OnBattleEndHandler()
    {
        _fsm.SpawnEvent((int)CharacterFSM.CharacterStateType.Idle);
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
