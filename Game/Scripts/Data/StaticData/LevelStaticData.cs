using System.Collections.Generic;
using UnityEngine;

namespace CharacterEditor.StaticData
{
    [CreateAssetMenu(fileName = "LevelData", menuName = "StaticData/Level")]
    public class LevelStaticData : ScriptableObject
    {
        public string LevelKey;

        public List<EntitySpawnerData> EnemySpawners;
        public List<EntitySpawnerData> PlayableNpcSpawners;
        public List<EntitySpawnerData> ContainerSpawners;
        public Vector3 InitialPlayerPoint;
    }
}