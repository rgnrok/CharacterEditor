using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Editor
{
    public class Preview : UnityEditor.Editor
    {
        public override bool HasPreviewGUI()
        {
            return true;
        }

        PreviewRenderUtility m_PreviewUtility;
        private GameObject m_PreviewInstance;
        private float m_BoundingVolumeScale;
        private float m_AvatarScale;
        private float m_ZoomFactor;
        private Vector2 m_PreviewDir = new Vector2(120, -20);
        private Vector3 m_PivotPositionOffset = Vector3.zero;
        protected ViewTool m_ViewTool = ViewTool.None;

        private PreviewRenderUtility previewUtility
        {
            get
            {
                if (m_PreviewUtility == null)
                {
                    m_PreviewUtility = new PreviewRenderUtility();
                    m_PreviewUtility.camera.fieldOfView = 30.0f;
                    m_PreviewUtility.camera.allowHDR = false;
                    m_PreviewUtility.camera.allowMSAA = false;
                    m_PreviewUtility.ambientColor = new Color(.1f, .1f, .1f, 0);
                    m_PreviewUtility.lights[0].intensity = 1.4f;
                    m_PreviewUtility.lights[0].transform.rotation = Quaternion.Euler(40f, 40f, 0);
                    m_PreviewUtility.lights[1].intensity = 1.4f;
                    m_PreviewUtility.camera.transform.position = new Vector3(1, 1f, -6);

                }

                return m_PreviewUtility;
            }
        }

        protected ViewTool viewTool
        {
            get
            {
                Event evt = Event.current;
                if (m_ViewTool == ViewTool.None)
                {
                    Debug.Log(string.Format("EVENT = btn:{0}, isScrollWheel:{1}", evt.button, evt.isScrollWheel));

                    bool controlKeyOnMac = (evt.control && Application.platform == RuntimePlatform.OSXEditor);
                    // actionKey could be command key on mac or ctrl on windows
                    bool actionKey = EditorGUI.actionKey;
                    bool noModifiers = (!actionKey && !controlKeyOnMac && !evt.alt);
                    if ((evt.button <= 0 && noModifiers) || (evt.button <= 0 && actionKey) || evt.button == 2)
                        m_ViewTool = ViewTool.Pan;
                    else if ((evt.button <= 0 && controlKeyOnMac) || (evt.button == 1 && evt.alt))
                        m_ViewTool = ViewTool.Zoom;
                    else if (evt.button <= 0 && evt.alt || evt.button == 1)
                        m_ViewTool = ViewTool.Orbit;

                }

                return m_ViewTool;
            }
        }

        public Vector3 bodyPosition
        {
            get
            {
                if (m_PreviewInstance != null)
                    return GetRenderableCenterRecurse(m_PreviewInstance, 1, 8);

                return Vector3.zero;

            }

        }

        public void InitInstance(GameObject go)
        {
            SetupBounds(go);
            m_PivotPositionOffset = Vector3.zero;
        }

        void SetupBounds(GameObject go)
        {
            if (go != null)
            {
                m_PreviewInstance = AddObjectToPreview(go);

                Bounds bounds = new Bounds(m_PreviewInstance.transform.position, Vector3.zero);
                GetRenderableBoundsRecurse(ref bounds, m_PreviewInstance);
                m_BoundingVolumeScale = Mathf.Max(bounds.size.x, Mathf.Max(bounds.size.y, bounds.size.z));
                m_AvatarScale = m_ZoomFactor = m_BoundingVolumeScale / 2;

            }

        }

        public static void GetRenderableBoundsRecurse(ref Bounds bounds, GameObject go)

        {
            // Do we have a mesh?
            MeshRenderer renderer = go.GetComponent(typeof(MeshRenderer)) as MeshRenderer;
            MeshFilter filter = go.GetComponent(typeof(MeshFilter)) as MeshFilter;
            if (renderer && filter && filter.sharedMesh)
            {
                // To prevent origo from always being included in bounds we initialize it
                // with renderer.bounds. This ensures correct bounds for meshes with origo outside the mesh.
                if (bounds.extents == Vector3.zero)
                    bounds = renderer.bounds;
                else
                    bounds.Encapsulate(renderer.bounds);
            }

            // Do we have a skinned mesh?
            SkinnedMeshRenderer skin = go.GetComponent(typeof(SkinnedMeshRenderer)) as SkinnedMeshRenderer;
            if (skin && skin.sharedMesh)
            {
                if (bounds.extents == Vector3.zero)
                    bounds = skin.bounds;
                else
                    bounds.Encapsulate(skin.bounds);
            }

            // Do we have a Sprite?
            SpriteRenderer sprite = go.GetComponent(typeof(SpriteRenderer)) as SpriteRenderer;
            if (sprite && sprite.sprite)
            {
                if (bounds.extents == Vector3.zero)
                    bounds = sprite.bounds;
                else
                    bounds.Encapsulate(sprite.bounds);
            }

            // Recurse into children
            foreach (Transform t in go.transform)
            {
                GetRenderableBoundsRecurse(ref bounds, t.gameObject);
            }
        }


        public static Vector3 GetRenderableCenterRecurse(GameObject go, int minDepth, int maxDepth)
        {
            Vector3 center = Vector3.zero;
            float sum = GetRenderableCenterRecurse(ref center, go, 0, minDepth, maxDepth);

            if (sum > 0)
                center = center / sum;
            else
                center = go.transform.position;

            return center;

        }

        private static float GetRenderableCenterRecurse(ref Vector3 center, GameObject go, int depth, int minDepth,
            int maxDepth)
        {
            if (depth > maxDepth) return 0;

            float ret = 0;
            if (depth > minDepth)
            {
                // Do we have a mesh?
                MeshRenderer renderer = go.GetComponent(typeof(MeshRenderer)) as MeshRenderer;
                MeshFilter filter = go.GetComponent(typeof(MeshFilter)) as MeshFilter;
                SkinnedMeshRenderer skin = go.GetComponent(typeof(SkinnedMeshRenderer)) as SkinnedMeshRenderer;
                SpriteRenderer sprite = go.GetComponent(typeof(SpriteRenderer)) as SpriteRenderer;

                if (renderer == null && filter == null && skin == null && sprite == null)
                {
                    ret = 1;
                    center = center + go.transform.position;
                }
                else if (renderer != null && filter != null)
                {
                    // case 542145, epsilon is too small. Accept up to 1 centimeter before discarding this model.
                    if (Vector3.Distance(renderer.bounds.center, go.transform.position) < 0.01F)
                    {
                        ret = 1;
                        center = center + go.transform.position;
                    }
                }

                else if (skin != null)
                {
                    // case 542145, epsilon is too small. Accept up to 1 centimeter before discarding this model.
                    if (Vector3.Distance(skin.bounds.center, go.transform.position) < 0.01F)
                    {
                        ret = 1;
                        center = center + go.transform.position;
                    }
                }

                else if (sprite != null)
                {
                    if (Vector3.Distance(sprite.bounds.center, go.transform.position) < 0.01F)
                    {
                        ret = 1;
                        center = center + go.transform.position;
                    }
                }
            }

            depth++;
            // Recurse into children
            foreach (Transform t in go.transform)
                ret += GetRenderableCenterRecurse(ref center, t.gameObject, depth, minDepth, maxDepth);

            return ret;
        }



        protected MouseCursor currentCursor
        {
            get
            {
                switch (m_ViewTool)
                {
                    case ViewTool.Orbit: return MouseCursor.Orbit;
                    case ViewTool.Pan: return MouseCursor.Pan;
                    case ViewTool.Zoom: return MouseCursor.Zoom;
                    default: return MouseCursor.Arrow;
                }
            }
        }

        protected void HandleMouseDown(Event evt, int id, Rect previewRect)
        {
            if (viewTool != ViewTool.None && previewRect.Contains(evt.mousePosition))
            {
                EditorGUIUtility.SetWantsMouseJumping(1);
                evt.Use();
                GUIUtility.hotControl = id;
            }
        }

        protected void HandleMouseUp(Event evt, int id)
        {
            if (GUIUtility.hotControl == id)
            {
                m_ViewTool = ViewTool.None;

                GUIUtility.hotControl = 0;
                EditorGUIUtility.SetWantsMouseJumping(0);
                evt.Use();
            }
        }

        protected void HandleMouseDrag(Event evt, int id, Rect previewRect)
        {
            if (m_PreviewInstance == null) return;
            if (GUIUtility.hotControl == id)
            {
                switch (m_ViewTool)
                {
                    case ViewTool.Orbit:
                        DoAvatarPreviewOrbit(evt, previewRect);
                        break;
                    case ViewTool.Pan:
                        //                    DoAvatarPreviewPan(evt);
                        DoAvatarPreviewOrbit(evt, previewRect);
                        break;

                    // case 605415 invert zoom delta to match scene view zooming
                    case ViewTool.Zoom:
                        DoAvatarPreviewZoom(evt, -HandleUtility.niceMouseDeltaZoom * (evt.shift ? 2.0f : 0.5f));
                        break;
                    default:
                        Debug.Log("Enum value not handled");
                        break;
                }
            }
        }


        protected void HandleViewTool(Event evt, EventType eventType, int id, Rect previewRect)
        {
            switch (eventType)
            {
                case EventType.ScrollWheel:
                    DoAvatarPreviewZoom(evt, HandleUtility.niceMouseDeltaZoom * (evt.shift ? 2.0f : 0.5f));
                    break;
                case EventType.MouseDown:
                    HandleMouseDown(evt, id, previewRect);
                    break;
                case EventType.MouseUp:
                    HandleMouseUp(evt, id);
                    break;
                case EventType.MouseDrag:
                    HandleMouseDrag(evt, id, previewRect);
                    break;
            }
        }

        public void DoAvatarPreviewOrbit(Event evt, Rect previewRect)
        {
            m_PreviewDir -= evt.delta * (evt.shift ? 3 : 1) / Mathf.Min(previewRect.width, previewRect.height) * 140.0f;
            m_PreviewDir.y = Mathf.Clamp(m_PreviewDir.y, -90, 90);
            evt.Use();
        }



        public void DoAvatarPreviewZoom(Event evt, float delta)
        {
            float zoomDelta = -delta * 0.05f;
            m_ZoomFactor += m_ZoomFactor * zoomDelta;
            // zoom is clamp too 10 time closer than the original zoom
            m_ZoomFactor = Mathf.Max(m_ZoomFactor, m_AvatarScale / 10.0f);
            evt.Use();
        }

        public void DoAvatarPreviewFrame(Event evt, EventType type, Rect previewRect)
        {
            if (type == EventType.KeyDown && evt.keyCode == KeyCode.F)
            {
                ResetPreviewFocus();
                m_ZoomFactor = m_AvatarScale;
                evt.Use();
            }
        }


        public void ResetPreviewFocus()
        {
            m_PivotPositionOffset = bodyPosition - m_PreviewInstance.transform.position;
        }

        public GameObject AddObjectToPreview(GameObject obj)
        {
            //        var instance = Instantiate(obj);
            //        //TODO: Remove unnessersary components
            //        previewUtility.AddSingleGO(instance);
            //        return instance;
            previewUtility.AddSingleGO(obj);
            return obj;
        }

        public override void OnPreviewGUI(Rect r, GUIStyle background)
        {
            DoAvatarPreview(r, background);
        }

        const string s_PreviewStr = "Preview";
        int m_PreviewHint = s_PreviewStr.GetHashCode();
        const string s_PreviewSceneStr = "PreviewSene";
        int m_PreviewSceneHint = s_PreviewSceneStr.GetHashCode();

        public void DoAvatarPreview(Rect rect, GUIStyle background)
        {
            //  Init();
            if (m_PreviewInstance == null) return;

            Rect previewRect = rect;
            int previewID = GUIUtility.GetControlID(m_PreviewHint, FocusType.Passive, previewRect);
            Event evt = Event.current;
            EventType type = evt.GetTypeForControl(previewID);

            if (type == EventType.Repaint)
            {
                DoRenderPreview(previewRect, background);
                previewUtility.EndAndDrawPreview(previewRect);
            }

            int previewSceneID = GUIUtility.GetControlID(m_PreviewSceneHint, FocusType.Passive);

            type = evt.GetTypeForControl(previewSceneID);

            HandleViewTool(evt, type, previewSceneID, previewRect);
            DoAvatarPreviewFrame(evt, type, previewRect);

            // Apply the current cursor
            if (evt.type == EventType.Repaint)
                EditorGUIUtility.AddCursorRect(previewRect, currentCursor);
        }

        public void DoRenderPreview(Rect previewRect, GUIStyle background)
        {
            if (m_PreviewInstance == null) return;
            var probe = RenderSettings.ambientProbe;
            previewUtility.BeginPreview(previewRect, background);

            Quaternion bodyRot;
            Vector3 bodyPos = m_PreviewInstance.transform.position;

            bodyRot = Quaternion.identity;
            SetupPreviewLightingAndFx(probe);

            Vector3 direction = bodyRot * Vector3.forward;
            direction[1] = 0;

            previewUtility.camera.nearClipPlane = 0.5f * m_ZoomFactor;
            previewUtility.camera.farClipPlane = 100.0f * m_AvatarScale;

            Quaternion camRot = Quaternion.Euler(-m_PreviewDir.y, -m_PreviewDir.x, 0);

            // Add panning offset
            Vector3 camPos = camRot * (Vector3.forward * -5.5f * m_ZoomFactor) + bodyPos + m_PivotPositionOffset;
            previewUtility.camera.transform.position = camPos;
            previewUtility.camera.transform.rotation = camRot;
            previewUtility.Render(false);
        }

        private void SetupPreviewLightingAndFx(SphericalHarmonicsL2 probe)
        {
            previewUtility.lights[0].intensity = 1.4f;
            previewUtility.lights[0].transform.rotation = Quaternion.Euler(40f, 40f, 0);
            previewUtility.lights[1].intensity = 1.4f;
            RenderSettings.ambientMode = AmbientMode.Custom;
            RenderSettings.ambientLight = new Color(0.1f, 0.1f, 0.1f, 1.0f);
            RenderSettings.ambientProbe = probe;
        }

        public void OnDisable()
        {
            if (m_PreviewUtility != null)
            {
                m_PreviewUtility.Cleanup();
                m_PreviewUtility = null;
            }
        }
    }
}