using CharacterEditor;
using CharacterEditor.CharacterInventory;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    [CustomEditor(typeof(EnemyConfig))]
    public class EnemyConfigEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("Open Wizard")) CreateEnemyWizard.CreateWizard();

            EditorUtility.SetDirty(target);
        }
    }
}