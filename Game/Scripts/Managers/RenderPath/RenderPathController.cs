using System.Collections.Generic;
using CharacterEditor;
using CharacterEditor.Services;
using UnityEngine;

public class RenderPathController : MonoBehaviour
{
    [SerializeField] private GameObject pathPoint;
    [SerializeField] private GameObject cursorPathPoint;
    [SerializeField] private GameObject endPathPoint;
    [SerializeField] private GameObject cursorAttackPathPoint;
    [SerializeField] private GameObject pathPointersContainer;
    [SerializeField] private GameObject movePointerInfo;

    [SerializeField] private GameObject debugPoint;
    [SerializeField] private bool isDebug;

    [SerializeField] private float distanceBewteenPoints = 0.5f;
    [SerializeField] private Vector3 pointYOffset = new Vector3(0, 2, 0);

    private bool _isEnabled;

    private ObjectPool<GameObject> _pointersPool;
    private ObjectPool<GameObject> _debugPool;
    private Character _currentCharacter;
    private GameObject _cursorPathPointInstance;
    private GameObject _endPathPointInstance;
    private GameObject _cursorAttackPathPointInstance;

    private MovePointerInfo _movePointerInfo;
    private IInputService _inputService;
    private ICharacterRenderPathService _renderPathService;
    private GameManager _gameManager;
    private ICharacterPathCalculationStrategy _pathCalculationStrategy;

    private void Start()
    {
        _pointersPool = new ObjectPool<GameObject>(CreatePathPoint, DestroyObject, HideObject);
        _debugPool = new ObjectPool<GameObject>(CreateDebugPoint, DestroyObject, HideObject);

        InstantiatePointer(out _cursorPathPointInstance, cursorPathPoint);
        InstantiatePointer(out _cursorAttackPathPointInstance, cursorAttackPathPoint);
        InstantiatePointer(out _endPathPointInstance, endPathPoint);

        _gameManager = GameManager.Instance;
        _gameManager.OnChangeCharacter += OnChangeCharacterHandler;

        _inputService = AllServices.Container.Single<IInputService>();
        _inputService.OnChangeMouseRaycastHit += OnChangeMouseRaycastHitHandler;

        _renderPathService = AllServices.Container.Single<ICharacterRenderPathService>();
        _renderPathService.OnStartDrawPath += OnStartDrawPathHandler;
    }

    private void OnDestroy()
    {
        if (_gameManager != null)
            _gameManager.OnChangeCharacter -= OnChangeCharacterHandler;

        if (_inputService != null)
            _inputService.OnChangeMouseRaycastHit -= OnChangeMouseRaycastHitHandler;

        if (_renderPathService != null)
            _renderPathService.OnStartDrawPath -= OnStartDrawPathHandler;
    }

    private void OnStartDrawPathHandler(Character character) => 
        SetCharacter(character);

    private void OnChangeMouseRaycastHitHandler(RaycastHit hit) => 
        RenderPath(hit);

    private void OnChangeCharacterHandler(Character character)
    {
        _isEnabled = _currentCharacter != null && character.Guid == _currentCharacter.Guid;
        if (!_isEnabled) Clean();
    }

    private void SetCharacter(Character character)
    {
        if (character?.Guid == _currentCharacter?.Guid) return;

        _currentCharacter = character;
        if (_currentCharacter == null)
        {
            Clean();
            return;
        }

        _pathCalculationStrategy = _currentCharacter.EntityGameObject.GetComponent<ICharacterPathCalculationStrategy>();
        _isEnabled = _gameManager.CurrentCharacter.Guid == _currentCharacter.Guid;
        _inputService.UpdateMouseHit();
    }

    private void Clean()
    {
        _debugPool.Reset();
        _pointersPool.Reset();
        _cursorPathPointInstance.SetActive(false);
        _cursorAttackPathPointInstance.SetActive(false);
        _endPathPointInstance.SetActive(false);
        _isEnabled = false;
    }

    private RenderPathInfo GetPathInfo(RaycastHit hit)
    {
        var pathInfo = new RenderPathInfo(hit.point);

        if (hit.collider.gameObject.layer == Constants.LAYER_ENEMY)
        {
            IAttacked attacked = _gameManager.GetEnemyByGoId(hit.collider.gameObject.GetInstanceID());
            if (attacked != null)
            {
                pathInfo.targetPoint = attacked.EntityGameObject.transform.position;
                pathInfo.endPoint = _currentCharacter.AttackComponent.GetTargetPointForAttack(attacked);
                pathInfo.attacked = attacked;
            }
        }

        return pathInfo;
    }

    private void RenderPath(RaycastHit hit)
    {
        if (!_isEnabled) return;
        if (_currentCharacter == null) return;
        if (distanceBewteenPoints < float.Epsilon) return;

        _pointersPool.Reset();
        _debugPool.Reset();

        var pathInfo = GetPathInfo(hit);

        if (pathInfo.attacked != null)
            RenderAttackPath(ref pathInfo);
        else
            RenderMovePath(ref pathInfo);


        _pointersPool.HideOthers();
        _debugPool.HideOthers();
    }

    private void PreparePointInfo(ref RenderPathInfo pointInfo)
    {
        if (distanceBewteenPoints < float.Epsilon) return;
        if (!_pathCalculationStrategy.CalculatePath(pointInfo.endPoint, out var corners))
            return;

        var characterAP = _currentCharacter.ActionPoints.StatCurrentValue;
        var hasPoints = true;

        var pointsList = new List<Vector3>(corners.Length);
        var prevPoint = corners[0];
        var currentDistance = 0f;
        for (var i = 1; i < corners.Length; i++)
        {
            var cornersDistance = Vector3.Distance(corners[i], corners[i - 1]);
            pointInfo.totalDistance += cornersDistance;

            if (!hasPoints) continue;

            var cornerPoint = corners[i];
            var direction = cornerPoint - prevPoint;
            var distanceBetweenCorners = direction.magnitude;
            var normalizeDirection = direction / distanceBetweenCorners;

            var isLastCorner = i == corners.Length - 1;
            var subPointsCount = isLastCorner
                ? Mathf.FloorToInt(distanceBetweenCorners / distanceBewteenPoints)
                : Mathf.RoundToInt(distanceBetweenCorners / distanceBewteenPoints);

            for (var sp = 0; sp < subPointsCount; sp++)
            {
                var pointPosition = prevPoint + normalizeDirection * distanceBewteenPoints;

                currentDistance += distanceBewteenPoints;
                prevPoint = pointPosition;

                var actionPointsCount = _currentCharacter.CalculateAP(currentDistance + distanceBewteenPoints);
                if (characterAP < actionPointsCount)
                {
                    hasPoints = false;
                    var maxDistance = _currentCharacter.Speed * characterAP;
                    var factor = (maxDistance - currentDistance) / distanceBewteenPoints;
                    pointInfo.endPoint = Vector3.Lerp(prevPoint, pointPosition, factor);

                    break;
                }

                pointsList.Add(pointPosition);
            }
        }

        pointInfo.isComplete = true;
        pointInfo.points = pointsList.ToArray();

        if (isDebug)
        {
            foreach (var corn in corners)
            {
                var debugGo = _debugPool.Get();
                debugGo.transform.position = corn + pointYOffset;
                debugGo.SetActive(true);
            }
        }
    }


    private void RenderMovePath(ref RenderPathInfo pathInfo)
    {
        _cursorAttackPathPointInstance.SetActive(false);
        _cursorPathPointInstance.transform.position = pathInfo.targetPoint + pointYOffset;
        _cursorPathPointInstance.SetActive(true);

        PreparePointInfo(ref pathInfo);

        if (!pathInfo.isComplete) return;

        foreach (var point in pathInfo.points)
        {
            var pointGo = _pointersPool.Get();
            pointGo.SetActive(true);
            pointGo.transform.position = point + pointYOffset;
        }

        var actionPointsCount = _currentCharacter.CalculateAP(pathInfo.totalDistance);
        UpdateMovePointerInfo(actionPointsCount, pathInfo.totalDistance);

        UpdateEndPathPoint(pathInfo);
    }

    private void RenderAttackPath(ref RenderPathInfo pathInfo)
    {
        if (pathInfo.attacked == null) return;
        
        _cursorPathPointInstance.SetActive(false);
        _cursorAttackPathPointInstance.transform.position = pathInfo.targetPoint + pointYOffset;
        _cursorAttackPathPointInstance.SetActive(true);

        var isRange = _currentCharacter.AttackComponent.IsRange();
        if (isRange)
        {
            //todo
            return;
        }

        pathInfo.endPoint = _currentCharacter.AttackComponent.GetTargetPointForAttack(pathInfo.attacked);
        // UpdateAttackedEndPoint(ref pathInfo);

        PreparePointInfo(ref pathInfo);

        if (!pathInfo.isComplete) return;
        // if (_currentCharacter.AttackComponent.IsAvailableDistance(pathInfo.totalDistance)) return;

        foreach (var point in pathInfo.points)
        {
            var pointGo = _pointersPool.Get();
            pointGo.SetActive(true);
            pointGo.transform.position = point + pointYOffset;
        }

        var actionPointsCount = _currentCharacter.CalculateAP(pathInfo.totalDistance, pathInfo.attacked);
        UpdateMovePointerInfo( actionPointsCount, pathInfo.totalDistance);

        UpdateEndPathPoint(pathInfo);
    }


    private void UpdateEndPathPoint(RenderPathInfo pathInfo)
    {
        _endPathPointInstance.transform.position = pathInfo.endPoint + pointYOffset;
        _endPathPointInstance.SetActive(true);
    }

    private void InstantiatePointer(out GameObject pointerInstance, GameObject prefab)
    {
        pointerInstance = Instantiate(prefab);
        pointerInstance.SetActive(false);
    }

    private void UpdateMovePointerInfo(int actionPoints, float distance)
    {
        _movePointerInfo = GetMovePointerInfo();
        if (_movePointerInfo == null) return;

        if (distance > float.Epsilon)
            _movePointerInfo.UpdateInfo(actionPoints, distance);
        else
            _movePointerInfo.UpdateFailInfo();
    }

    private MovePointerInfo GetMovePointerInfo()
    {
        if (_movePointerInfo == null)
            _movePointerInfo = Instantiate(movePointerInfo, _gameManager.Canvas).GetComponent<MovePointerInfo>();

        return _movePointerInfo;
    }

    #region Pool

    private GameObject CreateDebugPoint() => 
        Instantiate(debugPoint);

    private GameObject CreatePathPoint() => 
        Instantiate(pathPoint, pathPointersContainer != null ? pathPointersContainer.transform : null);

    private void HideObject(GameObject point) => 
        point.SetActive(false);

    private void DestroyObject(GameObject point) => 
        Destroy(point);

    #endregion

}
