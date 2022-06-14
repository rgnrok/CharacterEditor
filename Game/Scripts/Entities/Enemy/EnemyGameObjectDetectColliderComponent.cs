using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EnemySystem
{

    public class EnemyGameObjectDetectColliderComponent : MonoBehaviour
    {
        [SerializeField] private SphereCollider detectCollider;

        private readonly HashSet<int> _triggeredEntities = new HashSet<int>();
        private float _initColliderRadius;

        public event Action<GameObject> OnCharacterVisible;

        private void Awake()
        {
            if (detectCollider != null)
                _initColliderRadius = detectCollider.radius;
        }


        private void OnTriggerEnter(Collider other)
        {
            var goId = other.gameObject.GetInstanceID();
            if (_triggeredEntities.Contains(goId)) return;

            switch (other.gameObject.layer)
            {
                case Constants.LAYER_CHARACTER:
                    OnCharacterTriggered(other.gameObject);
                    break;
            }

            _triggeredEntities.Add(goId);
        }

        private void OnTriggerExit(Collider other)
        {
            var goId = other.gameObject.GetInstanceID();
            _triggeredEntities.Remove(goId);
        }

        private void OnCharacterTriggered(GameObject characterGo)
        {
            OnCharacterVisible?.Invoke(characterGo);
        }

        public void IncreaseDetectCollider()
        {
            if (detectCollider == null) return;
            detectCollider.radius = _initColliderRadius * 10;
        }

        public void DecreaseDetectCollider()
        {
            if (detectCollider == null) return;
            detectCollider.radius = _initColliderRadius;
        }
    }
}