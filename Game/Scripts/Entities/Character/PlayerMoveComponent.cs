using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

/*
 * Move with NavMesh Controller
 */
public class PlayerMoveComponent : MonoBehaviour
{
    public Animator _animator;
    public NavMeshAgent _navMeshAgent;
    public NavMeshObstacle _navMeshObstacle;

    private bool _isMoved;
    private bool _isRotated;
    private Coroutine _agentSwitchCoroutine;
    private Quaternion _targetRotation;
    private bool _disableNavMeshOnStop;

    public Action OnMoveCompleted;

    public float speed;

    private void Awake()
    {
        if (!_animator) _animator = GetComponentInChildren<Animator>();
        if (!_navMeshAgent) _navMeshAgent = GetComponent<NavMeshAgent>();
        if (!_navMeshObstacle) _navMeshObstacle = GetComponent<NavMeshObstacle>();
    }

    private void OnEnable()
    {
//       Stop(true); //todo
    }

    private void OnDisable()
    {
        if (_agentSwitchCoroutine != null) StopCoroutine(_agentSwitchCoroutine);
        _agentSwitchCoroutine = null;
    }

    private void Update()
    {
        UpdateAnimation();
        if (_isMoved && _navMeshAgent.remainingDistance < 0.1f && !_navMeshAgent.pathPending)
        {
            Stop();
        }

        if (_isRotated)
        {
            var angel = Helper.AngleBetweenObjects(transform.rotation, _targetRotation);
            if (angel < 10 && angel > -10) _isRotated = false;
            else
            {
                var rot = Quaternion.Slerp(transform.rotation, _targetRotation, speed * Time.deltaTime);
                transform.rotation = Quaternion.Euler(new Vector3(0f, rot.eulerAngles.y, 0f));
            }
        }

    }

    public void Stop(bool forceDisable = false)
    {
        if (_navMeshAgent == null) return;
        if (_navMeshAgent.enabled && _navMeshAgent.hasPath && _navMeshAgent.isOnNavMesh) _navMeshAgent.ResetPath();

        forceDisable |= _disableNavMeshOnStop;
        _isMoved = false;
        _disableNavMeshOnStop = false;

        if (_agentSwitchCoroutine != null) StopCoroutine(_agentSwitchCoroutine);
        if (forceDisable) _agentSwitchCoroutine = StartCoroutine(DisableNavmeshCoroutine());

        if (OnMoveCompleted != null) OnMoveCompleted();
    }

    public void MoveToPoint(Vector3 point, bool disableNavmeshOnStop)
    {
        _disableNavMeshOnStop = disableNavmeshOnStop;
        if (_agentSwitchCoroutine != null) StopCoroutine(_agentSwitchCoroutine);
        _agentSwitchCoroutine = StartCoroutine(Move(point));
    }

    public void EnableNavmesh()
    {
        if (_agentSwitchCoroutine != null) StopCoroutine(_agentSwitchCoroutine);
        _agentSwitchCoroutine = StartCoroutine(EnableNavmeshCoroutine());
    }

    public void DisableNavmesh()
    {
        if (_agentSwitchCoroutine != null) StopCoroutine(_agentSwitchCoroutine);
        _agentSwitchCoroutine = StartCoroutine(DisableNavmeshCoroutine());
    }

    private IEnumerator Move(Vector3 point)
    {
        RotateTo(point);
        yield return EnableNavmeshCoroutine();

        _navMeshAgent.SetDestination(point);
        _isMoved = true;
        _isRotated = false;
    }

    private IEnumerator EnableNavmeshCoroutine()
    {
        if (_navMeshObstacle == null || _navMeshAgent == null) yield break;
//        if (!_navMeshAgent.isOnNavMesh) yield break;

        if (!_navMeshAgent.enabled || _navMeshObstacle.enabled)
        {
            _navMeshObstacle.carving = false;
            yield return new WaitForEndOfFrame();
            _navMeshObstacle.enabled = false;
            yield return new WaitForEndOfFrame();

            _navMeshAgent.enabled = true;
            yield return new WaitForEndOfFrame();
        }
        _agentSwitchCoroutine = null;
    }

    private IEnumerator DisableNavmeshCoroutine()
    {
        if (_navMeshObstacle == null || _navMeshAgent == null) yield break;
//        if (!_navMeshAgent.isOnNavMesh) yield break;

        if (!_navMeshObstacle.enabled || _navMeshAgent.enabled)
        {
            _navMeshAgent.enabled = false;
            yield return new WaitForEndOfFrame();

            _navMeshObstacle.enabled = true;
            yield return new WaitForEndOfFrame();
            _navMeshObstacle.carving = true;
            yield return new WaitForEndOfFrame();
        }
        _agentSwitchCoroutine = null;
    }

    public void RotateTo(Vector3 lookPoint)
    {
        _targetRotation = Quaternion.LookRotation(lookPoint - transform.position);
        _isRotated = true;
    }

    private void UpdateAnimation()
    {
        var agentSpeed = 0f;
        if (_navMeshAgent != null && _navMeshAgent.enabled)
        {

            const float acceleration = 20f;
            const float deceleration = 30f;
            const float closeEnoughMeters = 4f;

            _navMeshAgent.acceleration =
                _navMeshAgent.remainingDistance < closeEnoughMeters ? deceleration : acceleration;

            agentSpeed = Vector3.Project(_navMeshAgent.desiredVelocity, transform.forward).magnitude;
        }

        _animator.SetFloat("speed", agentSpeed);
    }
}
