using System;
using TMPro;
using UnityEngine;

public class MovePointerInfo : MonoBehaviour
{
    public TextMeshProUGUI stateText;

    private Camera _camera;

    public void Start()
    {
        _camera = Camera.main;
        stateText.text = string.Empty;
    }

    public void UpdateInfo(int AP, float distance)
    {
        stateText.text = string.Format("{0}AP\n{1}m", AP, Math.Round(distance, 1));
    }

    public void UpdateFailInfo()
    {
        stateText.text = string.Format("Действие не возможно");
    }

    private void Update()
    {
        transform.LookAt(transform.position + _camera.transform.rotation * Vector3.forward,
            _camera.transform.rotation * Vector3.up);
    }
}
