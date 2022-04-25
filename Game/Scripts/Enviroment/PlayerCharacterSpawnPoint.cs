using UnityEngine;

public class PlayerCharacterSpawnPoint : MonoBehaviour
{
    [SerializeField]
    private string _characterConfigGuid;
    public string CharacterConfigGuid
    {
        get { return _characterConfigGuid; }
    }
}