using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FollowCamera : MonoBehaviour
{
    [SerializeField]
    [Range(0, 100)] private float smoothing = 0.1f;

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

    public event Action OnPositionChanged;
    public event Action OnZoomChanged;


    void Awake()
    {
        _camera = GetComponent<Camera>();
    }

    void Start()
    {
        _offset = transform.position;
    }


    public void SetFocus(Transform target, bool isFollow = false, bool force = false)
    {
        _followTarget = isFollow ? target : null;
        _offset.y = transform.position.y - target.position.y;

        if (force) ChangePosition(target.position + _offset);
    }

    void Update()
    {
        Debug.DrawRay(transform.position, transform.forward * 100, Color.cyan);

        if (_followTarget != null)
            FollowTarget();
        
        ScrollMap();
        ZoomMap();
    }

    private void FollowTarget()
    {
        if (_followTarget == null) return;

        _followPosition = _followTarget.position + _offset;
        if (transform.position != _followPosition)
        {
            ChangePosition(_followPosition);
        }
    }

    private void ScrollMap()
    {
        var uiObject = EventSystem.current.currentSelectedGameObject;
        if (uiObject != null && uiObject.GetComponent<InputField>() != null) return;

        var targetCamPosition = transform.position;

        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            targetCamPosition.x -= scrollSpeed * Time.deltaTime; // move on -X axis

        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            targetCamPosition.x += scrollSpeed * Time.deltaTime; // move on -X axis

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            targetCamPosition.z += scrollSpeed * Time.deltaTime; // move on -Z axis

        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            targetCamPosition.z -= scrollSpeed * Time.deltaTime; // move on -Z axis

        if (transform.position == targetCamPosition) return;

        _followTarget = null;
        ChangePosition(targetCamPosition);
    }

    private void ZoomMap()
    {
        var scrollWheelChange = Input.GetAxis("Mouse ScrollWheel");
        if (Helper.IsZero(scrollWheelChange)) return;

        var zoomDiff = zoomStep * Time.deltaTime;
        var focusedTargetPosition = transform.position - _offset;
        var zoomOffset = scrollWheelChange < 0 ? zoomDiff : -zoomDiff;

        var currentFov = _camera.fieldOfView;
        currentFov += zoomOffset;
        if (currentFov > maxCameraDistance || currentFov < minCameraDistance) return;

        _camera.fieldOfView = currentFov;

        var fovFactor = (currentFov - minCameraDistance) / (maxCameraDistance - minCameraDistance);
        var cameraOffsetDistance = (farYOffset - detailYOffset) * fovFactor + detailYOffset;
        _offset.y = cameraOffsetDistance;

        transform.rotation = Quaternion.Euler(Vector3.Lerp(detailCameraRotation, farCameraRotation, fovFactor));

        ChangePosition(focusedTargetPosition + _offset);
    }

    private void ChangePosition(Vector3 position)
    {
        transform.position = position;
        OnPositionChanged?.Invoke();
    }
}