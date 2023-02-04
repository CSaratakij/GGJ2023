using UnityEngine;
using UnityEditor;

namespace GameTools
{
    public class CollisionPlotter : EditorWindow
    {
        private const string COLLIDER_PARENT_NAME = "PlotterCollider";
        private const string COLLISION_PARENT_NAME = "PlotterCollision";

        public static int PressCount = 0;
        public static bool IsTrigger;
        public static bool IsBeginPlot;

        public enum ColliderType
        {
            None,
            BoxCollider,
            EdgeCollider
        }

        [SerializeField] private bool isUse;
        [SerializeField] private bool showAllCollision;
        [SerializeField] private bool isUseSnap;
        [SerializeField] private string colliderTag = "Untagged";
        [SerializeField] private int colliderLayer;
        [SerializeField] private ColliderType currentType;
        [SerializeField] private int currentColliderTab;
        [SerializeField] private string[] strAvailableCollider = new string[] { "None", "Box", "Edge" };

        private Vector3 beginPos;
        private Vector3 endPos;

        [MenuItem("Custom/Plotter/CollisionPlotter")]
        public static void ShowWindow()
        {
            var window = GetWindow(typeof(CollisionPlotter));
            window.titleContent = new GUIContent("CollisionPlotter");
        }

        private void OnEnable()
        {
            Tools.current = Tool.None;
            SceneView.duringSceneGui += OnSceneGUI;
        }

        private void OnDestroy()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
        }

        private void OnSceneGUI(SceneView sceneView)
        {
            if (isUse)
            {
                SceneInputHandler();
                PlotColliderHandler();
                SceneView.lastActiveSceneView.Repaint();
            }
        }

        void SceneInputHandler()
        {
            var e = Event.current;

            switch (e.type)
            {
                case EventType.KeyDown:
                    if (e.keyCode == KeyCode.C)
                    {
                        PressCount = 0;
                        IsBeginPlot = !IsBeginPlot;
                        Repaint();
                    }
                    else if (e.keyCode == KeyCode.S)
                    {
                        isUseSnap = !isUseSnap;
                        Repaint();
                    }

                    if (IsBeginPlot)
                    {
                        if (e.keyCode == KeyCode.T)
                        {
                            IsTrigger = !IsTrigger;
                            Repaint();
                        }
                    }
                    break;
            }

            var mousePos = e.mousePosition;
            var sceneViewCamera = SceneView.lastActiveSceneView.camera;

            mousePos.y = sceneViewCamera.pixelHeight - mousePos.y;
            mousePos = sceneViewCamera.ScreenToWorldPoint(mousePos);

            if (PressCount == 1)
            {

                Handles.color = Color.white;
                Handles.Label(mousePos + Vector2.up * 0.6f, (IsBeginPlot) ? "[ Using Ploter ]" : "");

                Handles.color = Color.yellow;
                Handles.Label(mousePos + Vector2.up * 0.5f, (isUseSnap) ? "Snap : On" : "Snap : Off");

                Handles.color = Color.red;
                Handles.DrawLine(beginPos, mousePos);
            }

            if (e.control)
            {
                PressCount = 0;
                Tools.current = Tool.None;

                Handles.color = Color.white;
                Handles.Label(mousePos + Vector2.up * 0.6f, "[ Delete Mode ]");

                Handles.color = Color.yellow;
                Handles.DrawWireCube(mousePos, new Vector2(0.5f, 0.5f));

                if (e.type == EventType.MouseDown && e.button == 0)
                {
                    DeleteColliderHandler(mousePos);

                    var controlId = GUIUtility.GetControlID(FocusType.Passive);
                    GUIUtility.hotControl = controlId;

                    Event.current.Use();
                }
            }
        }

        private void OnGUI()
        {
            GUIHandler();
        }

        private void GUIHandler()
        {
            GUILayout.Label("Setting", EditorStyles.boldLabel);

            currentColliderTab = GUILayout.Toolbar(currentColliderTab, strAvailableCollider);
            currentType = (ColliderType)currentColliderTab;

            isUse = ColliderType.None != currentType;
            IsBeginPlot = isUse;

            colliderTag = EditorGUILayout.TagField("Tag", colliderTag);
            colliderLayer = EditorGUILayout.LayerField("Layer", colliderLayer);

            AlwaysShowColliderHandler();
            IsTrigger = EditorGUILayout.Toggle("Use Trigger", IsTrigger);

            if (IsBeginPlot)
            {
                isUseSnap = EditorGUILayout.Toggle("Use Snap", isUseSnap);
            }
        }

        void AlwaysShowColliderHandler()
        {
            showAllCollision = EditorGUILayout.Toggle("Always Show Collider", showAllCollision);
            Physics2D.alwaysShowColliders = showAllCollision;
        }

        void PlotColliderHandler()
        {
            if (IsBeginPlot)
            {
                switch (currentType)
                {
                    case ColliderType.BoxCollider:
                        PlotBoxCollider();
                        break;

                    case ColliderType.EdgeCollider:
                        PlotEdgeCollider();
                        break;
                }
            }
        }

        void PlotBoxCollider()
        {
            var e = Event.current;

            switch (e.type)
            {
                case EventType.MouseDown:

                    if (e.button != 0)
                    {
                        if (e.button == 1)
                        {
                            PressCount = 0;
                        }
                        return;
                    }

                    PressCount += 1;

                    if (PressCount == 1)
                    {

                        var sceneViewCamera = SceneView.lastActiveSceneView.camera;
                        var mousePos = e.mousePosition;

                        mousePos.y = sceneViewCamera.pixelHeight - mousePos.y;
                        beginPos = sceneViewCamera.ScreenToWorldPoint(mousePos);

                        if (isUseSnap)
                        {
                            beginPos = Snap(1, beginPos);
                        }
                    }
                    else if (PressCount == 2)
                    {
                        var sceneViewCamera = SceneView.lastActiveSceneView.camera;
                        var mousePos = e.mousePosition;

                        mousePos.y = sceneViewCamera.pixelHeight - mousePos.y;
                        endPos = sceneViewCamera.ScreenToWorldPoint(mousePos);

                        if (isUseSnap)
                        {
                            endPos = Snap(1, endPos);
                        }

                        CreateBoxCollider2D(IsTrigger, beginPos, endPos);
                        SceneView.RepaintAll();

                        PressCount = 0;
                    }
                    break;
            }
        }

        void PlotEdgeCollider()
        {
            var e = Event.current;

            switch (e.type)
            {
                case EventType.MouseDown:

                    if (e.button != 0)
                    {
                        if (e.button == 1)
                        {
                            PressCount = 0;
                        }
                        return;
                    }

                    PressCount += 1;

                    if (PressCount == 1)
                    {

                        var sceneViewCamera = SceneView.lastActiveSceneView.camera;
                        var mousePos = e.mousePosition;

                        mousePos.y = sceneViewCamera.pixelHeight - mousePos.y;
                        beginPos = sceneViewCamera.ScreenToWorldPoint(mousePos);

                        if (isUseSnap)
                        {
                            beginPos = Snap(1, beginPos);
                        }
                    }
                    else if (PressCount == 2)
                    {
                        var sceneViewCamera = SceneView.lastActiveSceneView.camera;
                        var mousePos = e.mousePosition;

                        mousePos.y = sceneViewCamera.pixelHeight - mousePos.y;
                        endPos = sceneViewCamera.ScreenToWorldPoint(mousePos);

                        if (isUseSnap)
                        {
                            endPos = Snap(1, endPos);
                        }

                        CreateEdgeCollider2D(IsTrigger, beginPos, endPos);
                        SceneView.RepaintAll();

                        PressCount = 0;
                    }
                    break;
            }
        }

        void CreateBoxCollider2D(bool isTrigger, Vector3 beginPos, Vector3 endPos)
        {
            var objName = (isTrigger) ? (LayerMask.LayerToName(colliderLayer) + "_ground_collider") : (LayerMask.LayerToName(colliderLayer) + "_ground_collision");

            var obj = new GameObject(objName);
            var component = obj.AddComponent(typeof(BoxCollider2D)) as BoxCollider2D;

            var relativePos = (endPos - beginPos);
            var halfRelativePos = relativePos / 2.0f;

            var expectPos = beginPos + halfRelativePos;
            expectPos.z = 0.0f;

            var expectSize = halfRelativePos;
            obj.transform.position = expectPos;

            obj.tag = colliderTag;
            obj.layer = colliderLayer;

            component.isTrigger = isTrigger;
            component.size = new Vector2(Mathf.Abs(expectSize.x * 2.0f), Mathf.Abs(expectSize.y * 2.0f));

            var parentObjName = (isTrigger) ? COLLIDER_PARENT_NAME : COLLISION_PARENT_NAME;
            var parent = GameObject.Find(parentObjName);

            if (!parent)
            {
                parent = new GameObject(parentObjName);
                parent.transform.position = Vector3.zero;

                Undo.RegisterCreatedObjectUndo(parent, "Created parent of a new box collider 2D..");
            }

            obj.transform.SetParent(parent.transform);
            Undo.RegisterCreatedObjectUndo(obj, "Created a new box collider 2D..");
        }

        void CreateEdgeCollider2D(bool isTrigger, Vector3 beginPos, Vector3 endPos)
        {
            var objName = (isTrigger) ? (LayerMask.LayerToName(colliderLayer) + "_edge_collider") : (LayerMask.LayerToName(colliderLayer) + "_edge_collision");

            var obj = new GameObject(objName);
            var component = obj.AddComponent(typeof(EdgeCollider2D)) as EdgeCollider2D;

            var relativePos = (endPos - beginPos);
            var halfRelativePos = relativePos / 2.0f;

            var expectPos = beginPos + halfRelativePos;
            expectPos.z = 0.0f;

            obj.tag = colliderTag;
            obj.layer = colliderLayer;

            component.isTrigger = isTrigger;
            component.points = new Vector2[] {
            beginPos,
            endPos
        };

            var parentObjName = (isTrigger) ? COLLIDER_PARENT_NAME : COLLISION_PARENT_NAME;
            var parent = GameObject.Find(parentObjName);

            if (!parent)
            {
                parent = new GameObject(parentObjName);
                parent.transform.position = Vector3.zero;

                Undo.RegisterCreatedObjectUndo(parent, "Created parent of a new edge collider 2D..");
            }

            obj.transform.SetParent(parent.transform);
            Undo.RegisterCreatedObjectUndo(obj, "Created a new edge collider 2D..");
        }

        Vector3 Snap(float value, Vector3 target)
        {
            var depth = 0;
            var snapInverse = (1 / value);

            var result = Vector3.zero;

            result.x = Mathf.Round(target.x * value) / snapInverse;
            result.y = Mathf.Round(target.y * value) / snapInverse;
            result.z = depth;

            return result;
        }

        void DeleteColliderHandler(Vector3 origin)
        {
            var results = new Collider2D[1];
            Physics2D.OverlapBoxNonAlloc(origin, new Vector2(0.5f, 0.5f), 0.0f, results);

            if (results.Length <= 0)
            {
                return;
            }

            if (results[0] != null)
            {
                Undo.DestroyObjectImmediate(results[0].transform.gameObject);
            }
        }
    }
}

