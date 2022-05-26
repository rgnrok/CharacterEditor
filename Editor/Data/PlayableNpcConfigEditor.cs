using CharacterEditor;
using CharacterEditor.CharacterInventory;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    [CustomEditor(typeof(PlayableNpcConfig))]
    public class PlayableNpcConfigEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("Open Wizard")) PlayableNpcWizard.CreateWizard();

            EditorUtility.SetDirty(target);
        }
    }
}