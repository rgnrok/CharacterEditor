using EnemySystem;
using System.Collections.Generic;
using CharacterEditor.FmsPayload;

public class EnemyBattleFindTargetState : EnemyBattleBaseState
{
    private List<IBattleEntity> _characters;
    private EnemyAttackComponent _attackComponent;

    private bool _readyAttacket; //todo

    public EnemyBattleFindTargetState(EnemyBattleFSM fsm) : base(fsm)
    {
    }
    
    public override void Enter()
    {
        base.Enter();
        _characters = _fsm.Characters;
        _attackComponent = _enemy.AttackComponent;

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
        var battleEntity = _characters[0];
        if (_attackComponent.IsAvailableDistance(battleEntity))
        {
            _fsm.SpawnEvent((int)EnemyBattleFSM.EnemyBattleStateType.Attack, battleEntity);
            _readyAttacket = true;
            return;
        }

        var movePoint = _attackComponent.GetTargetPointForAttack(battleEntity);
        _fsm.SpawnEvent((int)EnemyBattleFSM.EnemyBattleStateType.Move, movePoint);
    }
}
