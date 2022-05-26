using UnityEngine;
using UnityEngine.EventSystems;

public class PointerYRotateComponent : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public float RotateSpeed = 0.3f;

    private bool _isRotated;
    private Vector3 _prevMousePosition;
    private Transform _targetTransform;

    public void SetTarget(Transform target)
    {
        _targetTransform = target;
    }

    private void OnDisable()
    {
        _targetTransform = null;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (_targetTransform == null) return;

        _isRotated = true;
        _prevMousePosition = Input.mousePosition;

        Cursor.visible = false;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _isRotated = false;
        Cursor.visible = true;
    }

    private void Update()
    {
        if (!_isRotated || _targetTransform == null) return;
            
        if (Input.GetMouseButton(0))
        {
            _targetTransform.Rotate(new Vector3(0, (Input.mousePosition.x - _prevMousePosition.x) * RotateSpeed, 0));
            _prevMousePosition = Input.mousePosition;
        }
    }
}
