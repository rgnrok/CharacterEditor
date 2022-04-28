using TMPro;
using UnityEngine;

public class EntityCanvas : MonoBehaviour
{
    public TextMeshProUGUI stateText;

    private Camera _camera;
    private bool _initialized;


    private void Update()
    {
        if (!_initialized) return;

        var rotation = _camera.transform.rotation;
        transform.LookAt(transform.position + rotation * Vector3.forward,
            rotation * Vector3.up);
    }

    public void Init(IBattleEntity entity)
    {
        entity.BaseFSM.OnCurrentStateChanged += OnCurrentStateChangedHandler;
        OnCurrentStateChangedHandler(entity.BaseFSM.CurrentState);

        _camera = Camera.main;
        _initialized = true;
    }

    private void OnCurrentStateChangedHandler(IExitableState state)
    {
        stateText.text = state.ToString();
    }
}
