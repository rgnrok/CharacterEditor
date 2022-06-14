using UnityEngine;

namespace CharacterEditor.Logic
{
    public enum SpawnType
    {
        Undefined,
        PlayableNpc,
        Enemy,
        Container,
    }

    public class SpawnMarker : MonoBehaviour
    {
        public string entityGuid;
        public SpawnType type;
    }
}