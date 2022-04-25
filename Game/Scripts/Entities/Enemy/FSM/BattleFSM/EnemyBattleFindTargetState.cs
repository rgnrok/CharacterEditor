using EnemySystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBattleFindTargetState : EnemyBattleBaseState<List<IBattleEntity>>
{
    private List<IBattleEntity> _entities;
    private EnemyAttackManager _attackManager;

    private bool _readyAttacket; //todo

    public EnemyBattleFindTargetState(EnemyBattleFSM fsm) : base(fsm)
    {
    }


    public new void Enter(List<IBattleEntity> targetEntity)
    {
        base.Enter(targetEntity);
        _entities = targetEntity;
        _attackManager = _enemy.AttackManager;

        AfterSwitching();
    }

    private void AfterSwitching()
    {
        FindTarget();
    }

    private void FindTarget()
    {
        if (_readyAttacket)
        {
            _fsm.SpawnEvent((int)EnemyBattleFSM.EnemyBattleStateType.TurnEnd);
            _readyAttacket = false;
            return;
        }
        var battleEntity = _entities[0];
        if (_attackManager.IsAvailableDistance(battleEntity))
        {
            _fsm.SpawnEvent((int)EnemyBattleFSM.EnemyBattleStateType.Attack, battleEntity);
            _readyAttacket = true;
            return;
        }

        _fsm.SpawnEvent((int)EnemyBattleFSM.EnemyBattleStateType.Move, _attackManager.GetTargetPointForAttack(battleEntity));
    }
}
