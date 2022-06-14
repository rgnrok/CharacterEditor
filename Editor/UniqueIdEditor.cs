using System;
using System.Linq;
using CharacterEditor.Logic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Editor
{
    [CustomEditor(typeof(UniqueId))]
    public class UniqueIdEditor : UnityEditor.Editor
    {
        private void OnEnable()
        {
            var uniqId = (UniqueId)target;

            if (IsPrefab(uniqId)) return;

            if (string.IsNullOrEmpty(uniqId.Id))
            {
                Generate(uniqId);
            }
            else
            {
                var uids = FindObjectsOfType<UniqueId>();
                if (uids.Any(other => other != uniqId && other.Id == uniqId.Id))
                    Generate(uniqId);
            }
        }

        private void Generate(UniqueId uniqueId)
        {
            uniqueId.Id = $"{uniqueId.gameObject.scene.name}_{Guid.NewGuid().ToString()}";
            if (!Application.isPlaying)
            {
                EditorUtility.SetDirty(uniqueId);
                EditorSceneManager.MarkSceneDirty(uniqueId.gameObject.scene);
            }
        }

        private bool IsPrefab(UniqueId uniqueId) =>
            uniqueId.gameObject.scene.rootCount == 0;
    }
}