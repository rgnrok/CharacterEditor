using CharacterEditor;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

class CharacterNavMeshPathCalculation : ICharacterPathCalculation
{
    private readonly NavMeshPath _path;

    private Character _character;
    private NavMeshAgent _navMeshAgent;

    private NavMeshAgent NavMeshAgent
    {
        get
        {
            if (_navMeshAgent == null)
                _navMeshAgent = _character.EntityGameObject.GetComponent<NavMeshAgent>();

            return _navMeshAgent;
        }
    }

    public CharacterNavMeshPathCalculation()
    {
        _path = new NavMeshPath();
    }

    public void SetCharacter(Character character)
    {
        _character = character;
    }

    public bool CalculatePath(Vector3 to, out Vector3[] points)
    {
        if (!NavMeshAgent.enabled)
        {
            points = new Vector3[0];
            return false;
        }

        var result = NavMeshAgent.CalculatePath(to, _path);
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
}