using UnityEngine;

public class ContainerSpawnPoint : MonoBehaviour
{
    [SerializeField]
    private string _containerDataGuid;
    public string ContainerDataGuid
    {
        get { return _containerDataGuid; }
    }
}
