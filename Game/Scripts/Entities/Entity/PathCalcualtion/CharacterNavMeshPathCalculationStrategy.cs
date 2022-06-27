using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CharacterNavMeshPathCalculationStrategy : MonoBehaviour, ICharacterPathCalculationStrategy
{
    [SerializeField] private NavMeshAgent _navMeshAgent;

    private readonly Dictionary<int, NavMeshObstacle> _obstacleCache = new Dictionary<int, NavMeshObstacle>(10);
    private NavMeshPath _path;

    private void Awake()
    {
        if (_navMeshAgent == null)
            _navMeshAgent = GetComponent<NavMeshAgent>();

        _path = new NavMeshPath();
    }

    public void Reset()
    {
        _obstacleCache.Clear();
    }

    public bool CalculatePath(Vector3 to, out Vector3[] points)
    {
        if (!_navMeshAgent.enabled)
        {
            points = new Vector3[0];
            return false;
        }

        var result = _navMeshAgent.CalculatePath(to, _path);
        if (!result || _path.corners.Length < 2)
        {
            points = new Vector3[0];
            return false;
        }

        points = _path.corners;
        return true;
    }

    public float PathDistance(Vector3 endPoint)
    {
        if (!CalculatePath(endPoint, out var points))
            return -1;

        var distance = 0f;
        for (var i = 1; i < points.Length; i++)
            distance += Vector3.Distance(points[i - 1], points[i]);

        return distance;
    }

    public Vector3 GetAttackPoint(AttackComponent attackComponent, GameObject target)
    {
        var point = target.transform.position;

        if (_navMeshAgent == null || !_navMeshAgent.enabled) return point;

        var hasObstacles = _navMeshAgent.Raycast(point, out var navMeshHit);
        if (!hasObstacles)
        {
            Debug.LogError("Not find obstacle. Enemy must have obstacle!");
            return point;
        }

        DrawCircle(navMeshHit.position, 0.5f, Color.black);

        // if target is full visible
        // var distance = (point - navMeshHit.position).magnitude;
        // if (distance < 1)
        // {
        // if (TryFindFullVisibleAttackPoint(point, attackComponent.AttackDistance, distance, navMeshHit.position, out var hitPosition))
        //         return hitPosition;
        // }


        if (!FindClosestAttackPoint(target, out var attackPoint))
        {
            Debug.LogError("FindClosestAttackPoint not found!");
            return point;
        }

        DrawCircle(attackPoint, 0.5f, Color.magenta);
        var distance = (point - attackPoint).magnitude;
        if (distance > 1)
        {
            Debug.LogError("After FindClosestAttackPoint distance more 1!");
            return point;
        }

        if (TryFindFullVisibleAttackPoint(point, attackComponent.AttackDistance, distance, attackPoint, out var resultPosition))
            return resultPosition;

        Debug.LogError("GetAttackPoint not find point!");
        return point;
    }

    private bool FindClosestAttackPoint(GameObject target, out Vector3 resultPoint)
    {
        var targetPoint = target.transform.position;
        resultPoint = targetPoint;

        var obstacle = GetTargetObstacle(target);

        var forwardDirection = (targetPoint - _navMeshAgent.transform.position).normalized;
        var backDirection = forwardDirection * -1;
        var rightDirection = Quaternion.Euler(0, 90, 0) * forwardDirection;
        var leftDirection = rightDirection * -1;

        var minDistance = float.MaxValue;
        var dirs = new[] {forwardDirection, backDirection, rightDirection, leftDirection};
        var obstacleRadius = obstacle != null ? obstacle.radius*1.5f : 0.5f;

        var isPointFound = false;
        Vector3 prevCorner = targetPoint;
        foreach (var direction in dirs)
        {
            var findPoint = targetPoint - direction * obstacleRadius;
            if (!NavMesh.FindClosestEdge(findPoint, out var hit, NavMesh.AllAreas)) continue;

            DrawCircle(findPoint, hit.distance, Color.red);
            DrawCircle(hit.position, hit.distance, Color.green);
            if (!CalculatePath(hit.position, out var corners)) continue;

            var sqrDistance = 0f;
            for (var i = 1; i < corners.Length; i++)
                sqrDistance += Vector3.SqrMagnitude(corners[i] - corners[i - 1]);

            if (sqrDistance < minDistance)
            {
                minDistance = sqrDistance;
                resultPoint = hit.position;
                isPointFound = true;
                prevCorner = corners[corners.Length - 2];
            }
        }

        if (!isPointFound) return false;

        // not found obstacle
        if (!NavMesh.Raycast(prevCorner, targetPoint, out var navMeshHit, NavMesh.AllAreas))
            return false;

        resultPoint = navMeshHit.position;

        return true;
    }

    private NavMeshObstacle GetTargetObstacle(GameObject target)
    {
        var key = target.GetInstanceID();
        if (!_obstacleCache.TryGetValue(key, out var obstacle))
            _obstacleCache[key] = obstacle = target.GetComponent<NavMeshObstacle>();

        return obstacle;
    }

    private bool TryFindFullVisibleAttackPoint(Vector3 point, float attackDistance, float distance, Vector3 obstaclePosition, out Vector3 resultPosition)
    {
        var direction = (point - _navMeshAgent.transform.position).normalized;
        var rayFrom = _navMeshAgent.transform.position + Vector3.up;

        Debug.DrawRay(rayFrom, direction, Color.red);
        var layer = 1 << Constants.LAYER_ENEMY | 1 << Constants.LAYER_WALL | 1 << Constants.LAYER_CHARACTER;
        if (Physics.Raycast(rayFrom, direction, out var hit, 100, layer))
        {
            switch (hit.collider.gameObject.layer)
            {
                case Constants.LAYER_ENEMY:
                case Constants.LAYER_CHARACTER:
                    if (hit.collider.transform.position != point) break;

                    resultPosition = distance < attackDistance
                        ? point - direction * attackDistance
                        : obstaclePosition;
                    return true;
            }
        }

        resultPosition = point;
        return false;
    }

    private void DrawCircle(Vector3 center, float radius, Color color)
    {
        var prevPos = center + new Vector3(radius, 0, 0);
        for (int i = 0; i < 30; i++)
        {
            var angle = (float)(i + 1) / 30.0f * Mathf.PI * 2.0f;
            var newPos = center + new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);
            Debug.DrawLine(prevPos, newPos, color);
            prevPos = newPos;
        }
    }
}