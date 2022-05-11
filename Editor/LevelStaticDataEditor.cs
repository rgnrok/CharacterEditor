using System.Collections.Generic;
using System.Linq;
using CharacterEditor.Logic;
using CharacterEditor.StaticData;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace Editor
{
    [CustomEditor(typeof(LevelStaticData))]
    public class LevelStaticDataEditor : UnityEditor.Editor
    {
        private const string InitialPointTag = "InitialPoint";

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var levelData = (LevelStaticData) target;

            if (GUILayout.Button("Collect"))
            {
                levelData.EnemySpawners = SelectSpawners(levelData, SpawnType.Enemy);
                levelData.PlayableNpcSpawners = SelectSpawners(levelData, SpawnType.PlayableNpc);
                levelData.ContainerSpawners = SelectSpawners(levelData, SpawnType.Container);

                levelData.LevelKey = SceneManager.GetActiveScene().name;
                levelData.InitialPlayerPoint = GameObject.FindWithTag(InitialPointTag).transform.position;
            }

            EditorUtility.SetDirty(target);
        }

        private List<EntitySpawnerData> SelectSpawners(LevelStaticData levelData, SpawnType type)
        {
            return FindObjectsOfType<SpawnMarker>()
                    .Where(x => x.type == type)
                    .Select(x => new EntitySpawnerData(x.entityGuid, x.transform.position))
                    .ToList();

        }
    }
}