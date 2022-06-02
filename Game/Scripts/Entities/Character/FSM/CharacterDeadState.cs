using CharacterEditor;
using UnityEngine;

public class CharacterDeadState : IState
{
    private readonly Character _character;

    public CharacterDeadState(CharacterFSM fsm)
    {
        _character = fsm.Character;
    }

    public void Enter()
    {
        Die();
    }

    public void Exit()
    {
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
