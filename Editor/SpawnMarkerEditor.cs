using System;
using CharacterEditor.Logic;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    [CustomEditor(typeof(SpawnMarker))]
    public class SpawnMarkerEditor : UnityEditor.Editor
    {
        [DrawGizmo(GizmoType.Active | GizmoType.Pickable | GizmoType.NonSelected)]
        public static void DrawCustomGizmo(SpawnMarker spawner, GizmoType gizmoType)
        {
            if (spawner == null) return;

            Gizmos.color = GetColorByType(spawner.type);
            Gizmos.DrawSphere(spawner.transform.position, 0.5f);

            var id = $"{spawner.type}-{spawner.entityGuid.Substring(0, 8)}";
            DrawString(id, spawner.transform.position + Vector3.up * 2);

        }

        private static void DrawString(string text, Vector3 worldPos, Color? textColor = null, Color? backColor = null)
        {
            Handles.BeginGUI();
            var restoreTextColor = GUI.color;
            var restoreBackColor = GUI.backgroundColor;

            GUI.color = textColor ?? Color.white;
            GUI.backgroundColor = backColor ?? Color.black;

            var view = SceneView.currentDrawingSceneView;
            var screenPos = view.camera.WorldToScreenPoint(worldPos);
            var size = GUI.skin.label.CalcSize(new GUIContent(text));

            if (view != null && view.camera != null)
            {
                if (screenPos.y < 0 || screenPos.y > Screen.height || screenPos.x < 0 || screenPos.x > Screen.width || screenPos.z < 0)
                {
                    GUI.color = restoreTextColor;
                    Handles.EndGUI();
                    return;
                }

                var r = new Rect(screenPos.x - (size.x / 2), -screenPos.y + view.position.height + 4, size.x, size.y);
                GUI.Box(r, text, EditorStyles.numberField);
                GUI.Label(r, text);
                GUI.color = restoreTextColor;
                GUI.backgroundColor = restoreBackColor;
            }
            Handles.EndGUI();
        }

        private static Color GetColorByType(SpawnType type)
        {
            switch (type)
            {
                case SpawnType.PlayableNpc:
                    return Color.green;
                case SpawnType.Enemy:
                    return Color.red;
                case SpawnType.Container:
                    return Color.yellow;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

    }
}