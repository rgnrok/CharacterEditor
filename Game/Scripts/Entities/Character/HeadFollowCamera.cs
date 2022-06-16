using UnityEngine;

namespace CharacterEditor
{
    public class HeadFollowCamera : MonoBehaviour
    {
        [SerializeField] private Vector3 _offset;
        private Transform _target;
        private IConfigManager _configManager;

        private void Awake()
        {
            _configManager = AllServices.Container.Single<IConfigManager>();
            _configManager.OnChangeCharacter += ChangeTarget;
        }

        private void OnDestroy()
        {
            if (_configManager != null)
                _configManager.OnChangeCharacter -= ChangeTarget;
        }

        private void LateUpdate()
        {
            if (_target != null) transform.position = _target.transform.position + _offset;
        }

        private void ChangeTarget()
        {
            _target = _configManager.ConfigData.GetHead();
        }
    }
}