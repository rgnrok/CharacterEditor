using UnityEngine;

public class EnemySpawnPoint : MonoBehaviour
{
    [SerializeField]
    private string _enemyGuid;
    public string EnemyGuid
    {
        get { return _enemyGuid; }
    }
}
