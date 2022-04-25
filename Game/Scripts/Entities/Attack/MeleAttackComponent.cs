using System;
using System.Collections;
using CharacterEditor;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class MeleAttackComponent : AttackComponent
{
    protected override float AttackDistance { get { return 2f; } }

    public override void Attack(IAttacked enity, Action completeHandler)
    {
        MoveComponent.RotateTo(enity.EntityGameObject.transform.position);
        Animator.SetTrigger(Constants.CHARACTER_MELEE_ATTACK_1_TRIGGER); //todo

        StartCoroutine(AttackCoroutine(enity, completeHandler));
    }


    private IEnumerator AttackCoroutine(IAttacked enity, Action completeHandler)
    {
        yield return new WaitForSecondsRealtime(2f);
        var dmg = (enity is Character) ? 50 : Random.Range(10, 20);//todo
        enity.Health.StatCurrentValue -= dmg;
        completeHandler?.Invoke();
    }

    public override Vector3 GetTargetPointForAttack(Vector3 target)
    {
        if (MoveComponent == null) return base.GetTargetPointForAttack(target);

        var path = new NavMeshPath();
        NavMesh.CalculatePath(MoveComponent.transform.position, target, NavMesh.AllAreas, path);
        if (path.status != NavMeshPathStatus.PathComplete || path.corners.Length < 2)
            return base.GetTargetPointForAttack(target);

        var direction = Helper.GetDirection(path.corners[path.corners.Length-2], target);
        //todo check if corner distance less that attack distance ?
        return target - direction * (AttackDistance - 0.1f);
    }
}
