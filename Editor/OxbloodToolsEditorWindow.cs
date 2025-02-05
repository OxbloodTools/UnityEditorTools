using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
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

            OxbloodToolsEditorWindow window = GetWindow<OxbloodToolsEditorWindow>();
            GUIContent tabIcon = EditorGUIUtility.IconContent("ToolHandleGlobal");
            tabIcon.text = "Oxblood Tools";
            window.titleContent = tabIcon;
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
            int totalObjects = 0;
            int totalTriangles = 0;
            int totalVerts = 0;

            for (int i = 0; i < SceneManager.sceneCount; i++) //if multiple scenes are open, search em all
            {
                Scene scene = SceneManager.GetSceneAt(i);

                GameObject[] rootObjects = scene.GetRootGameObjects();
                totalObjects += rootObjects.Length;

                // Get the children as well
                foreach (GameObject root in rootObjects)
                {
                    //add to total object count
                    totalObjects += CountAllChildren(root.transform);
                    //Get all the triangeles in each mesh by grabbing attached mesh filters
                    MeshFilter[] meshFilters = root.GetComponentsInChildren<MeshFilter>(true);
                    foreach (MeshFilter mf in meshFilters)
                    {
                        if (mf.sharedMesh != null)
                        {
                            totalVerts += mf.sharedMesh.vertexCount;
                            totalTriangles += mf.sharedMesh.triangles.Length / 3;
                        }
                    }
                }
            }

            static int CountAllChildren(Transform parent)
            {
                int count = 0;
                foreach (Transform child in parent)
                {
                    count++;
                    count += CountAllChildren(child);
                }

                return count;
            }

            _statRefreshBar.highValue = totalObjects;
            _statTotalObjectCount.text = totalObjects.ToString("N0");
            _statTotalVertexCount.text = totalVerts.ToString("N0");
            _statTotalTriangleCount.text = totalTriangles.ToString("N0");
            _statRefreshBar.value = 0;
        }

        #endregion

        void TerrainTriCount()
        {
            int totalTriangles = 0;
            
            Terrain[] terrains = GameObject.FindObjectsOfType<Terrain>();
            foreach (Terrain terrain in terrains)
            {
                TerrainData data = terrain.terrainData;
                int w = data.heightmapResolution - 1;
                int h = data.heightmapResolution - 1;
                totalTriangles += (w * h * 2); // 2 triangles per terrain quad
            }
        }
    }
}