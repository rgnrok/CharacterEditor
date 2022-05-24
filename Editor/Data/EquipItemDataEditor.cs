using CharacterEditor.CharacterInventory;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    [CustomEditor(typeof(EquipItemData))]
    public class EquipItemDataEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("Open Wizard")) EquipItemWizard.CreateWizard();

            EditorUtility.SetDirty(target);
        }
    }
}