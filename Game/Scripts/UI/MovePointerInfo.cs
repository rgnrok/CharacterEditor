using System;
using TMPro;
using UnityEngine;

public class MovePointerInfo : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI stateText;
    [SerializeField] private Vector3 _movePointerInfoOffset = new Vector3(100, 10, 0);

    private Camera _camera;

    public void Start()
    {
        _camera = Camera.main;
        stateText.text = string.Empty;
    }

    private void Update()
    {
        var rotation = _camera.transform.rotation;
        transform.LookAt(transform.position + rotation * Vector3.forward,
            rotation * Vector3.up);
    }

    public void UpdateInfo(int AP, float distance)
    {
        UpdatePosition();
        var meters = Math.Round(distance, 1);
        stateText.text = $"{AP}AP\n{meters}m";
    }

    public void UpdateFailInfo()
    {
        UpdatePosition();
        stateText.text = "Действие не возможно";
    }

    private void UpdatePosition()
    {
        transform.position = Input.mousePosition + _movePointerInfoOffset;
    }
}
