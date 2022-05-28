using CharacterEditor;
using UnityEngine;

public class CharacterIdleState : CharacterBaseState
{
    public CharacterIdleState(CharacterFSM fsm) : base(fsm)
    {
    }

    public override void Enter()
    {
        base.Enter();
        GameManager.Instance.PlayerMoveController.OnGroundClick += OnGroundClickHandler;
    }

    public override void Exit()
    {
        base.Exit();
        GameManager.Instance.PlayerMoveController.OnGroundClick -= OnGroundClickHandler;
    }


    protected override void OnEnemyClick(IAttacked attacked)
    {
        _fsm.SpawnEvent((int)CharacterFSM.CharacterStateType.Attack, attacked);
    }

    private void OnGroundClickHandler(string characterGuid, Vector3 point)
    {
        if (_character == null || _character.Guid != characterGuid) return;

        _fsm.SpawnEvent((int)CharacterFSM.CharacterStateType.Move, point);
    }
}
