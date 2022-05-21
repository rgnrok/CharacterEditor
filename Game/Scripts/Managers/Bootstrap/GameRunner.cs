using UnityEngine;

namespace CharacterEditor
{
    public class GameRunner : MonoBehaviour
    {
        public GameObject bootstrapPrefab;

        void Awake()
        {
            var bootstrapper = FindObjectOfType<GameBootstrapper>();
            if (bootstrapper) return;

            Instantiate(bootstrapPrefab);
        }
    }
}