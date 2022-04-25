using System.Collections;
using System.Collections.Generic;
using CharacterEditor;
using UnityEngine;

public class CharacterDeadState : CharacterBaseState
{
    public CharacterDeadState(CharacterFSM fsm) : base(fsm)
    {
    }

    public new void Enter()
    {
        base.Enter();
        Die();
    }

    private void Die()
    {
        _character.GameObjectData.Animator.SetTrigger(Constants.CHARACTER_DIE_TRIGGER);
        
        var deadCollider = _character.EntityGameObject.transform.Find("DeadCollider");
        var liveCollider = _character.EntityGameObject.GetComponent<CapsuleCollider>();

        if (deadCollider != null) deadCollider.gameObject.SetActive(true);
        if (liveCollider != null) liveCollider.enabled = false;
    }
}
