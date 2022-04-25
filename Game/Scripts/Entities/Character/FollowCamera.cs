using System;
using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FollowCamera : MonoBehaviour
{
    [SerializeField]
    [Range(0, 1)] private float smoothing = 0.1f;

    [SerializeField] private float minCameraDistance = 10f;
    [SerializeField] private float maxCameraDistance = 30f;
    [SerializeField] private float zoomStep = 1f;
    [SerializeField] private float scrollSpeed = 5f;
    [SerializeField] private Vector3 detailCameraRotation;
    [SerializeField] private Vector3 farCameraRotation;
    [SerializeField] private float detailYOffset;
    [SerializeField] private float farYOffset;

    private Transform _followTarget;
    private Vector3 _followPosition;
    private Vector3 _offset;
    private Camera _camera;

    private float _cameraOffsetDistance;
    private Vector3 _targetPosition;
    private Vector3 _targetRotation;
    private float _targetFieldOfView;

    private bool _positionChanged;

    public event Action OnPositionChanged;
    public event Action OnZoomChanged;

    void Awake()
    {
        _camera = GetComponent<Camera>();
    }

    void Start()
    {
        _targetPosition = transform.position;
        _targetRotation = transform.rotation.eulerAngles;
        _targetFieldOfView = _camera.fieldOfView;
        _offset = _targetPosition;

        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, Mathf.Infinity, 1 << Constants.LAYER_GROUND))
            _cameraOffsetDistance = transform.position.y - hit.point.y;

        //StartCoroutine(UpdateYPosition()); // tmp disable need change logic
    }


    public void SetFocus(Transform target, bool isFollow = false, bool forse = false)
    {
        _followTarget = isFollow ? target : null;
        _offset.y = transform.position.y - target.position.y;

        ChangePosition(target.position + _offset, forse);
    }

    void Update()
    {
        Debug.DrawRay(transform.position, transform.forward * 100, Color.cyan);

        if (_followTarget != null)
            FollowTarget();
        
        ScrollMap();
        ZoomMap();

        UpdateCameraPosition();

        if (Mathf.Abs(_camera.fieldOfView - _targetFieldOfView) > 0.1f)
            Camera.main.fieldOfView = Mathf.Lerp(_camera.fieldOfView, _targetFieldOfView, smoothing);

        if (Vector3.Distance(transform.rotation.eulerAngles, _targetRotation) > 0.1f)
            transform.rotation = Quaternion.Euler(Vector3.Lerp(transform.rotation.eulerAngles, _targetRotation, smoothing));
    }
   

    protected void FollowTarget()
    {
        if (_followTarget == null) return;

        _followPosition = _followTarget.position + _offset;
        if (transform.position != _followPosition)
        {
            ChangePosition(_followPosition);
        }
    }

    protected void ScrollMap()
    {
        var uiObject = EventSystem.current.currentSelectedGameObject;
        if (uiObject != null && uiObject.GetComponent<InputField>() != null) return;

        var targetCamPosition = _targetPosition;

        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            targetCamPosition.x -= scrollSpeed * Time.deltaTime; // move on -X axis
        }
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            targetCamPosition.x += scrollSpeed * Time.deltaTime; // move on -X axis
        }
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            targetCamPosition.z += scrollSpeed * Time.deltaTime; // move on -Z axis
        }
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            targetCamPosition.z -= scrollSpeed * Time.deltaTime; // move on -Z axis
        }

        if (_targetPosition != targetCamPosition)
        {
            _followTarget = null;
            ChangePosition(targetCamPosition);
        }
    }

    protected IEnumerator UpdateYPosition()
    {
        while (true)
        {
            //Change y position
            var groundLayer = 1 << Constants.LAYER_GROUND;
            groundLayer |= 1 << Constants.LAYER_WALL; //Not ignore wall
            RaycastHit hit;
            if (Physics.Raycast(_targetPosition, transform.forward, out hit, Mathf.Infinity, groundLayer))
            {
                var yPosition = _cameraOffsetDistance + hit.point.y;
                if (Mathf.Abs(_targetPosition.y - yPosition) > 0.5f)
                {
                    _targetPosition.y = _cameraOffsetDistance + hit.point.y;

                    var dz = transform.position.z - _targetPosition.z;
                    _targetPosition.z += (dz < 0 ? 1 : -1) * 0.1f;
                }
            }

            yield return new WaitForSeconds(1);
        }
    }
   

    protected void ZoomMap()
    {
        if (Helper.IsZero(Input.GetAxis("Mouse ScrollWheel"))) return;

        var _focusedTargetPosition = _targetPosition - _offset;


        var zoomOffset = 0f;
        if (Input.GetAxis("Mouse ScrollWheel") < 0 && _targetFieldOfView <= maxCameraDistance)
            zoomOffset += zoomStep;
        if (Input.GetAxis("Mouse ScrollWheel") > 0 && _targetFieldOfView >= minCameraDistance)
            zoomOffset -= zoomStep;

        if (Helper.IsZero(zoomOffset)) return;

        _targetFieldOfView += zoomOffset;
        var factor = (_targetFieldOfView - minCameraDistance) / (maxCameraDistance - minCameraDistance);

        _cameraOffsetDistance = (farYOffset - detailYOffset) * factor + detailYOffset;
        _offset.y = _cameraOffsetDistance;
        _targetRotation = Vector3.Lerp(detailCameraRotation, farCameraRotation, factor);
        ChangePosition(_focusedTargetPosition + _offset);
    }

    private void ChangePosition(Vector3 position, bool force = false)
    {
        _targetPosition = position;
        if (force)
        {
            transform.position = _targetPosition;
        }

        _positionChanged = !force;
        OnPositionChanged?.Invoke();
    }

    private void UpdateCameraPosition()
    {
        if (!_positionChanged) return;

        var sqrDist = (_targetPosition - transform.position).sqrMagnitude;
        if (sqrDist > 0.005f)
        {
            transform.position = Vector3.Lerp(transform.position, _targetPosition, smoothing);
        }
        else
        {
            transform.position = _targetPosition;
            _positionChanged = false;
        }
    }
}