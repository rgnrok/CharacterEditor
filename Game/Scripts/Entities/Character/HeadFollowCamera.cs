using UnityEngine;

namespace CharacterEditor
{
    public class HeadFollowCamera : MonoBehaviour
    {
        public Vector3 offset;
        private Transform target;
        private IConfigManager _configManager;

        void Awake()
        {
            _configManager = AllServices.Container.Single<IConfigManager>();
        }

        private void Start()
        {
            if (_configManager != null)
                _configManager.OnChangeCharacter += ChangeTarget;
        }

        private void LateUpdate()
        {
            if (target != null) transform.position = target.transform.position + offset;
        }

        private void ChangeTarget()
        {
            target = _configManager.ConfigData.GetHead();
        }
    }
}