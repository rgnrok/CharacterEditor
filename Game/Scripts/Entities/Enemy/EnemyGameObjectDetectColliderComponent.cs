using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EnemySystem
{

    public class EnemyGameObjectDetectColliderComponent : MonoBehaviour
    {
        [SerializeField] private SphereCollider detectCollider;
        private PlayerMoveComponent moveComponent;

        private HashSet<int> triggeredEntities = new HashSet<int>();
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
            if (triggeredEntities.Contains(goId)) return;

            switch (other.gameObject.layer)
            {
                case Constants.LAYER_CHARACTER:
                    OnCharacterTriggered(other.gameObject);
                    break;
            }

            triggeredEntities.Add(goId);
        }

        private void OnTriggerExit(Collider other)
        {
            var goId = other.gameObject.GetInstanceID();
            triggeredEntities.Remove(goId);
        }

        private void OnCharacterTriggered(GameObject characterGo)
        {
            if (OnCharacterVisible != null) OnCharacterVisible(characterGo);
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