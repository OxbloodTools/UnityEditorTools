using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Oxblood.editor
{
    public class OxbloodToolsEditorWindow : EditorWindow
    {
        //UI Components
        private bool _enableMoveToMousePosition;
        private Button _selectObjectsByComponentButton;
        private Button _selectObjectsByLayerButton;
        private Button _refreshStatsButton;
        private DropdownField _layerDropdown;

        // Variables
        private ProgressBar _statRefreshBar;
        private Label _statTotalObjectCount;
        private Label _statTotalTriangleCount;
        private Label _statTotalVertexCount;


        [MenuItem("Oxblood/Tools")]
        public static void ShowWindow()
        {
            GetWindow<OxbloodToolsEditorWindow>("Oxblood Tools", true, typeof(EditorWindow));
        }

        private void OnEnable()
        {
            VisualTreeAsset visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(StaticData.UiComponentsPath + "OxbloodToolsWindow.uxml");
            visualTree.CloneTree(rootVisualElement);

            //FEATURE: Move To Mouse
            Toggle toggle = rootVisualElement.Q<Toggle>("enableMoveToMouseToggle");

            _enableMoveToMousePosition = EditorPrefs.GetBool("EnableMoveToMousePosition", true);
            toggle.value = _enableMoveToMousePosition;
            toggle.RegisterValueChangedCallback(evt => _enableMoveToMousePosition = evt.newValue);

            //FEATURE: Select objects by component
            _selectObjectsByComponentButton = rootVisualElement.Q<Button>("_selectObjectsByTypeButton");

            //FEATURE Select objects by layer
            _selectObjectsByLayerButton = rootVisualElement.Q<Button>("_selectObjectsByLayerButton");

            // Populate associated dropdown with the projects layers (need to do this if a new layer gets added?).
            _layerDropdown = rootVisualElement.Q<DropdownField>("layerDropdown");
            _layerDropdown.choices = UnityEditorInternal.InternalEditorUtility.layers.ToList();
            _layerDropdown.value = _layerDropdown.choices[0];

            //Hierarchy

            //Display and refresh scene stats
            _statRefreshBar = rootVisualElement.Q<ProgressBar>("_statRefreshBar");
            _refreshStatsButton = rootVisualElement.Q<Button>("_refreshStatsButton");
            _statTotalObjectCount = rootVisualElement.Q<Label>("_statTotalObjectCount");
            _statTotalTriangleCount = rootVisualElement.Q<Label>("_statTotalTriangleCount");
            _statTotalVertexCount = rootVisualElement.Q<Label>("_statTotalVertexCount");


            //Subscribe to events
            SceneView.duringSceneGui += OnSceneGUI;
            _selectObjectsByComponentButton.clicked += SelectObjectsWithComponent;
            _selectObjectsByLayerButton.clicked += SelectObjectsByLayer;
            _refreshStatsButton.clicked += RefreshStats;
        }

        private void OnDisable()
        {
            EditorPrefs.SetBool("EnableMoveToMousePosition", _enableMoveToMousePosition);

            SceneView.duringSceneGui -= OnSceneGUI;
            _selectObjectsByComponentButton.clicked -= SelectObjectsWithComponent;
            _selectObjectsByLayerButton.clicked -= SelectObjectsByLayer;
            _refreshStatsButton.clicked -= RefreshStats;
        }

        private void OnSceneGUI(SceneView sceneView)
        {
            if (_enableMoveToMousePosition && Event.current.keyCode == KeyCode.Space)
            {
                MoveSelectedObjectToMousePosition();
                Event.current.Use();
            }
        }

        #region Logic Methods

        private static void MoveSelectedObjectToMousePosition()
        {
            GameObject selectedObject = Selection.activeGameObject;

            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Undo.RecordObject(selectedObject.transform, "Move Object to Mouse Position");
                selectedObject.transform.position = hit.point;
            }
        }

        private static void SelectObjectsWithComponent()
        {
            Object[] objectsWithComponent = FindObjectsByType<Object>(FindObjectsSortMode.None);

            Selection.objects = objectsWithComponent;
        }

        private void SelectObjectsByLayer()
        {
            string layerName = _layerDropdown.value;
            int layerIndex = LayerMask.NameToLayer(layerName);

            GameObject[] allObjects = GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None); //Have to gather everything first?  Might be expensive on large scenes
            Object[] objectsInLayer = allObjects.Where(obj => obj.layer == layerIndex).ToArray<Object>();

            // Perform the actual selection
            Selection.objects = objectsInLayer;
        }

        private void RefreshStats()
        {
            int totalObjectCount = Resources.FindObjectsOfTypeAll<Transform>().Length;
            int totalTriangles = 0;
            int totalVerts = 0;

            //total object count
            Transform[] totalObjects = Resources.FindObjectsOfTypeAll<Transform>();

            //total tris and verts
            MeshFilter[] statTotalMeshCount = Resources.FindObjectsOfTypeAll<MeshFilter>();
            _statRefreshBar.highValue = totalObjects.Length;
            foreach (MeshFilter mesh in statTotalMeshCount)
            {
                if (mesh.sharedMesh != null)
                {
                    totalVerts += mesh.sharedMesh.triangles.Length;
                    totalTriangles += mesh.sharedMesh.triangles.Length / 3;
                    _statRefreshBar.value++;
                }
            }

            _statTotalObjectCount.text = totalObjectCount.ToString("N0");
            _statTotalVertexCount.text = totalVerts.ToString("N0");
            _statTotalTriangleCount.text = totalTriangles.ToString("N0");
            _statRefreshBar.value = 0;
        }

        #endregion
    }
}