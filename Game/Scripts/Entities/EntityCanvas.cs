using TMPro;
using UnityEngine;

public class EntityCanvas : MonoBehaviour
{
    public TextMeshProUGUI stateText;

    private Camera _camera;
    private bool _inited;


    public void Init(IBattleEntity entity)
    {
        entity.BaseFSM.OnCurrentStateChanged += OnCurrentStateChangedHandler;
        OnCurrentStateChangedHandler(entity.BaseFSM.CurrentState);

        _camera = Camera.main;
        _inited = true;
    }

    private void OnCurrentStateChangedHandler(IExitableState state)
    {
        // stateText.text = state.GetName();
    }

    private void Update()
    {
        if (!_inited) return;

        transform.LookAt(transform.position + _camera.transform.rotation * Vector3.forward,
            _camera.transform.rotation * Vector3.up);
    }
}
