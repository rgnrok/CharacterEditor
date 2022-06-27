using UnityEngine;

public interface ICharacterPathCalculationStrategy
{
    bool CalculatePath(Vector3 to, out Vector3[] points);

    float PathDistance(Vector3 endPoint);

    Vector3 GetAttackPoint(AttackComponent attackComponent, GameObject point);
}
