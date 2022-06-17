using CharacterEditor;
using System.Collections;
using System.Collections.Generic;
using CharacterEditor.Services;
using UnityEngine;
using UnityEngine.AI;
using Logger = CharacterEditor.Logger;

public class RenderPathController : MonoBehaviour
{
    protected class PathInfo
    {
        public Vector3 targetPoint;
        public List<Vector3> points;
        public Vector3 endPoint;
        public bool isComplete;
        public float totalDistance;
        public float availableDistance;
        public IAttacked attacked;

        public PathInfo(Vector3 point)
        {
            points = new List<Vector3>();
            targetPoint = point;
            isComplete = false;
            totalDistance = 0;
            availableDistance = 0;
        }
    }

    [SerializeField] private GameObject pathPoint;
    [SerializeField] private GameObject cursorPathPoint;
    [SerializeField] private GameObject endPathPoint;
    [SerializeField] private GameObject cursorAttackPathPoint;
    [SerializeField] private GameObject pathPointersContainer;
    [SerializeField] private GameObject movePointerInfo;


    [SerializeField] private GameObject debugPoint;
    [SerializeField] private bool isDebug;

    [SerializeField] private float distanceBewteenPoints;
    [SerializeField] private Vector3 pointSubVector = new Vector3(0, 2, 0);

    private ObjectPool<GameObject> _pointersPool;
    private ObjectPool<GameObject> _debugPool;
    private Character _character;
    private PlayerMoveComponent _moveComponent;
    private GameObject _cursorPathPointInstance;
    private GameObject _endPathPointInstance;
    private GameObject _cursorAttackPathPointInstance;
    private bool _isEnabled;

    private MovePointerInfo _movePonterInfo;
    private Vector3 _movePointerInfoOffset;
    private IInputService _inputService;
    private ICharacterRenderPathService _renderPathService;

    void Start()
    {
        _pointersPool = new ObjectPool<GameObject>(CreatePathPoint, DestroyObject, HideObject);
        _debugPool = new ObjectPool<GameObject>(CreateDebugPoint, DestroyObject, HideObject);

        _cursorPathPointInstance = Instantiate(cursorPathPoint);
        _cursorPathPointInstance.SetActive(false);

        _cursorAttackPathPointInstance = Instantiate(cursorAttackPathPoint);
        _cursorAttackPathPointInstance.SetActive(false);

        _endPathPointInstance = Instantiate(endPathPoint);
        _endPathPointInstance.SetActive(false);

        _movePointerInfoOffset = new Vector3(100, 10, 0);

        GameManager.Instance.OnChangeCharacter += OnChangeCharacterHandler;
        _inputService = AllServices.Container.Single<IInputService>();
        _inputService.OnChangeMouseRaycastHit += OnChangeMouseRaycastHitHandler;

        _renderPathService = AllServices.Container.Single<ICharacterRenderPathService>();
        _renderPathService.OnStartDrawPath += OnStartDrawPathHandler;
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnChangeCharacter -= OnChangeCharacterHandler;

        if (_inputService != null)
            _inputService.OnChangeMouseRaycastHit -= OnChangeMouseRaycastHitHandler;

        if (_renderPathService != null)
            _renderPathService.OnStartDrawPath -= OnStartDrawPathHandler;
    }

    private void OnStartDrawPathHandler(Character character)
    {
        SetCharacter(character);
    }


    private GameObject CreateDebugPoint()
    {
        return Instantiate(debugPoint);
    }
    private GameObject CreatePathPoint()
    {
        return Instantiate(pathPoint, pathPointersContainer != null ? pathPointersContainer.transform : null);
    }

    private void HideObject(GameObject point)
    {
        point.SetActive(false);
    }

    private void DestroyObject(GameObject point)
    {
        Destroy(point);
    }

    private void OnChangeMouseRaycastHitHandler(RaycastHit hit)
    {
        RenderPath(hit);
    }

    private MovePointerInfo GetMovePoint()
    {
        if (_movePonterInfo == null)
            _movePonterInfo = Instantiate(movePointerInfo, GameManager.Instance.Canvas).GetComponent<MovePointerInfo>();

        return _movePonterInfo;
    }

    private void UpdateMovePointer(int actionPoints, float distance)
    {
        _movePonterInfo = GetMovePoint();
        if (_movePonterInfo == null) return;

        _movePonterInfo.transform.position = Input.mousePosition + _movePointerInfoOffset;
        if (distance == -1)
            _movePonterInfo.UpdateFailInfo();
        else
            _movePonterInfo.UpdateInfo(actionPoints, distance);
    }


    private void SetCharacter(Character ch)
    {
        if (_character != null && ch!=null && ch.Guid == _character.Guid )
            return;
        
        _character = ch;
        if (_character == null)
        {
            Clean();
            return;
        }

        _moveComponent = _character.GameObjectData.CharacterObject.GetComponent<PlayerMoveComponent>();
        _isEnabled = GameManager.Instance.CurrentCharacter.Guid == _character.Guid;
    }

    private void OnChangeCharacterHandler(Character ch)
    {
        _isEnabled = _character != null && ch.Guid == _character.Guid;
        if (!_isEnabled) Clean();
    }

    private void Clean()
    {
        _debugPool.Reset();
        _pointersPool.Reset();
        _cursorPathPointInstance.SetActive(false);
        _cursorAttackPathPointInstance.SetActive(false);
        _endPathPointInstance.SetActive(false);
    }

    private void RenderPath(RaycastHit hit)
    {
        if (_character == null || _moveComponent == null || !_isEnabled) return;

        var point = hit.point;
        _pointersPool.Reset();
        _debugPool.Reset();

        IAttacked attacked = null;
        int actionPointsCount = -1;
        if (hit.collider.gameObject.layer == Constants.LAYER_ENEMY)
        {
            attacked = GameManager.Instance.GetEnemyByGoId(hit.collider.gameObject.GetInstanceID());
            if (attacked != null)
            {
                _cursorAttackPathPointInstance.transform.position = attacked.EntityGameObject.transform.position + pointSubVector;
                _cursorAttackPathPointInstance.SetActive(true);
                _cursorPathPointInstance.SetActive(false);

                if (_character.AttackComponent.IsAvailableDistance(attacked)) return;

                point = _character.AttackComponent.GetTargetPointForAttack(attacked);
            }
        }
        else
        {
            _cursorAttackPathPointInstance.SetActive(false);
        }

        var path = new NavMeshPath();
        NavMesh.CalculatePath(_moveComponent.transform.position, point, NavMesh.AllAreas, path);

        var corners = path.corners;
        if (corners.Length < 2)
        {
            Logger.LogWarning("Invalid path");
            UpdateMovePointer(-1, -1);
            return;
        }

        if (distanceBewteenPoints < float.Epsilon) return;


        var prevPoint = corners[0];
        Vector3 pointPosition;
        Vector3 endPoint = corners[corners.Length - 1];

        var characterAP = _character.ActionPoints.StatCurrentValue;

        var totalDistance = 0f;
        var currentDistance = 0f;
        var hasPoints = true;
        for (int i = 0; i < corners.Length -1 ; i++)
        {
            var distanceBetweenCorners = (corners[i + 1] - corners[i]).magnitude;

            var heading =  (corners[i + 1] - prevPoint);
            var direction = heading / heading.magnitude;

            var subPointsCount = Mathf.RoundToInt(distanceBetweenCorners / distanceBewteenPoints);
            if (i == corners.Length - 2) subPointsCount = Mathf.FloorToInt(distanceBetweenCorners / distanceBewteenPoints);

            totalDistance += distanceBetweenCorners;
            // Debug.Log("totalDistance " + totalDistance + " subPointsCount " + subPointsCount);
            for (var sp = 0; sp < subPointsCount && hasPoints; sp++)
            {
                pointPosition = prevPoint + direction * distanceBewteenPoints;
                currentDistance += distanceBewteenPoints;
            
                var pointGo = _pointersPool.Get();
                pointGo.SetActive(true);
                pointGo.transform.position = pointPosition + pointSubVector;

                prevPoint = pointPosition;

                //check if next point is bad
                if (attacked == null)
                {
                    actionPointsCount = _character.CalculateAP(currentDistance + distanceBewteenPoints);
                    if (characterAP < actionPointsCount)
                    {
                        hasPoints = false;
                        var maxDistance = _character.Speed * characterAP;
                        var factor = (maxDistance - (sp+1) * distanceBewteenPoints) / distanceBewteenPoints;
                        endPoint = Vector3.Lerp(pointPosition, pointPosition + direction * distanceBewteenPoints, factor) + pointSubVector;
                        break;
                    }
                }
            }

            if (isDebug)
            {
                var debugGo = _debugPool.Get();
                debugGo.transform.position = corners[i] + pointSubVector;
                debugGo.SetActive(true);
            }
        }
        if (hasPoints) endPoint = corners[corners.Length - 1] + pointSubVector;


        if (isDebug)
        {
            var debugGo = _debugPool.Get();
            debugGo.transform.position = corners[corners.Length-1] + pointSubVector;
            debugGo.SetActive(true);
        }


        if (attacked != null)
        {
            actionPointsCount = _character.CalculateAP(totalDistance, attacked);
            UpdateMovePointer(actionPointsCount, totalDistance);


            _endPathPointInstance.transform.position = point + pointSubVector;
            _endPathPointInstance.SetActive(true);
        }
        else
        {
            _cursorPathPointInstance.transform.position = corners[corners.Length - 1] + pointSubVector;
            _cursorPathPointInstance.SetActive(true);

            _endPathPointInstance.transform.position = endPoint;
            _endPathPointInstance.SetActive(true);

            actionPointsCount = _character.CalculateAP(totalDistance);
            UpdateMovePointer(actionPointsCount, totalDistance);
        }

        _pointersPool.HiddeOthers();
        _debugPool.HiddeOthers();
    }
    

    protected PathInfo GetPathInfo(RaycastHit hit)
    {
        PathInfo pathInfo = new PathInfo(hit.point);

        pathInfo.endPoint = hit.point;
        if (hit.collider.gameObject.layer == Constants.LAYER_ENEMY)
        {
            IAttacked attacked = GameManager.Instance.GetEnemyByGoId(hit.collider.gameObject.GetInstanceID());
            if (attacked != null)
            {
                pathInfo.targetPoint = attacked.EntityGameObject.transform.position;
                pathInfo.endPoint = _character.AttackComponent.GetTargetPointForAttack(attacked);
                pathInfo.attacked = attacked;
            }
        }

        return pathInfo;
    }

    private void PrefaprePointInfo(ref PathInfo pointInfo)
    {
        var path = new NavMeshPath();
        NavMesh.CalculatePath(_moveComponent.transform.position, pointInfo.endPoint, NavMesh.AllAreas, path);

        var corners = path.corners;

        if (corners.Length < 2) return;
        if (distanceBewteenPoints < float.Epsilon) return;

        int actionPointsCount;
        var prevPoint = corners[0];
        Vector3 pointPosition;

        var characterAP = _character.ActionPoints.StatCurrentValue;
        var hasPoints = true;
        for (int i = 0; i < corners.Length - 1; i++)
        {
            var distanceBetweenCorners = (corners[i + 1] - corners[i]).magnitude;
            var heading = (corners[i + 1] - prevPoint);
            var direction = heading / heading.magnitude;

            var subPointsCount = Mathf.RoundToInt(distanceBetweenCorners / distanceBewteenPoints);
            if (i == corners.Length - 2)
                subPointsCount = Mathf.FloorToInt(distanceBetweenCorners / distanceBewteenPoints);

            pointInfo.totalDistance += distanceBetweenCorners;

            for (var sp = 0; sp < subPointsCount && hasPoints; sp++)
            {
                pointPosition = prevPoint + direction * distanceBewteenPoints;
                pointInfo.availableDistance += distanceBewteenPoints;

                pointInfo.points.Add(pointPosition);
                prevPoint = pointPosition;


                actionPointsCount = _character.CalculateAP(pointInfo.availableDistance + distanceBewteenPoints);
                if (characterAP < actionPointsCount)
                {
                    hasPoints = false;
                    var maxDistance = _character.Speed * characterAP;
                    var factor = (maxDistance - (sp + 1) * distanceBewteenPoints) / distanceBewteenPoints;
                    pointInfo.endPoint = Vector3.Lerp(pointPosition, pointPosition + direction * distanceBewteenPoints,
                        factor);
                    break;
                }


            }

            if (isDebug)
            {
                var debugGo = _debugPool.Get();
                debugGo.transform.position = corners[i] + pointSubVector;
                debugGo.SetActive(true);
            }
        }


        if (isDebug)
            {
                var debugGo = _debugPool.Get();
                debugGo.transform.position = corners[corners.Length - 1] + pointSubVector;
                debugGo.SetActive(true);
            }
            if (hasPoints) pointInfo.endPoint = corners[corners.Length - 1] ;

    }


    private void RenderMovePath(PathInfo pathInfo)
    {
        _cursorAttackPathPointInstance.SetActive(false);

    }

    private void RenderAttackPath(PathInfo pathInfo)
    {
        if (pathInfo.attacked == null) return;
        
        _cursorPathPointInstance.SetActive(false);
        _cursorAttackPathPointInstance.transform.position = pathInfo.targetPoint + pointSubVector;
        _cursorAttackPathPointInstance.SetActive(true);

        var isRange = _character.AttackComponent.IsRange();
        if (isRange)
        {
            //todo
            return;
        }


        PrefaprePointInfo(ref pathInfo);

        if (!pathInfo.isComplete) return;
        if (_character.AttackComponent.IsAvailableDistance(pathInfo.totalDistance)) return;

        foreach (var pathPoint in pathInfo.points)
        {
            var pointGo = _pointersPool.Get();
            pointGo.SetActive(true);
            pointGo.transform.position = pathPoint + pointSubVector;

        }


        var actionPointsCount = _character.CalculateAP(pathInfo.totalDistance, pathInfo.attacked);
        UpdateMovePointer( actionPointsCount, pathInfo.totalDistance);


        _endPathPointInstance.transform.position = pathInfo.endPoint + pointSubVector;
        _endPathPointInstance.SetActive(true);
    }

    private void RenderPath2(RaycastHit hit)
    {
        if (_character == null || _moveComponent == null || !_isEnabled) return;

        _pointersPool.Reset();
        _debugPool.Reset();


        var pathInfo = GetPathInfo(hit);



        if (pathInfo.attacked != null)
        {
            RenderAttackPath(pathInfo);
            return;
        }
        RenderMovePath(pathInfo);



        _pointersPool.HiddeOthers();
        _debugPool.HiddeOthers();
    }


}
