using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using Button = UnityEngine.UIElements.Button;

namespace Oxblood.editor
{
    public class OxbloodAssetsEditorWindow : EditorWindow
    {
        private Button _refreshLibrary;
        private Button _refreshLibraryFull;
        private VisualElement _imageContainer;
        private AssetGrabber _assetGrabber;

        [MenuItem("Oxblood/Assets")]
        public static void ShowWindow()
        {
            GetWindow<OxbloodAssetsEditorWindow>("Oxblood Assets", true, typeof(EditorWindow));
        }

        private void OnEnable()
        {
            VisualTreeAsset visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/com.oxblood.oxbloodtools/UI/OxbloodAssetsWindow.uxml");
            visualTree.CloneTree(rootVisualElement);

            StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Packages/com.oxblood.oxbloodtools/UI/OxbloodStyle.uss");
            rootVisualElement.styleSheets.Add(styleSheet);


            _refreshLibrary = rootVisualElement.Q<Button>("_refreshLibrary");
            _refreshLibraryFull = rootVisualElement.Q<Button>("_refreshLibraryFull");
            _refreshLibrary.clicked += RebuildLibrary;
            _refreshLibraryFull.clicked += RebuildLibraryFull;
            

            _imageContainer = rootVisualElement.Q<VisualElement>("_galleryWindow");

            _assetGrabber = ScriptableObject.CreateInstance<AssetGrabber>(); //the class that handles all the library data


            InitialiseOxbloodTools.Initialise();
            RefreshGalleryView();
        }

        private void OnDisable()
        {
            _refreshLibrary.clicked -= RebuildLibrary;
            _refreshLibraryFull.clicked -= RebuildLibraryFull;
        }

        private void RebuildLibrary()
        {
            _assetGrabber.RebuildOxbloodAssetDatabase(false);
            RefreshGalleryView();
        }
        private void RebuildLibraryFull()
        {
            _assetGrabber.RebuildOxbloodAssetDatabase(true);
            RefreshGalleryView();
        }
        

        private void RefreshGalleryView()
        {
            _imageContainer.Clear();

            string[] imgGuids = AssetDatabase.FindAssets("t:Texture2D", new[] { StaticPaths.OxbloodGeneratedData });

            foreach (string imgGuid in imgGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(imgGuid);
                Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);

                VisualElement container = new VisualElement();
                container.AddToClassList("galleryItem");

                VisualElement content = new VisualElement
                {
                    style =
                    {
                        backgroundImage = texture,
                    }
                };
                content.AddToClassList("content-container");

                //extrapolate prefab GUID by using the filename of the associated image
                string objName = TrimUntilLastUnderscore(Path.GetFileNameWithoutExtension(AssetDatabase.GUIDToAssetPath(Path.GetFileNameWithoutExtension(path)))); //frankly ridiculous stringification
                container.userData = objName; //stick the linked objects data inside the container
                container.RegisterCallback<ClickEvent>(_ => SpawnPrefabByGuid(Path.GetFileNameWithoutExtension(path)));

                //add label
                Label nameLabel = new Label(objName);
                nameLabel.AddToClassList("assetLabel");

                container.Add(content);
                container.Add(nameLabel);
                _imageContainer.Add(container);
            }
        }


        private static string TrimUntilLastUnderscore(string input)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input)); //this should never fire hopefully, just trying to do best practices

            int lastUnderscoreIndex = input.LastIndexOf('_');
            return lastUnderscoreIndex == -1 ? input : input.Substring(lastUnderscoreIndex + 1);
        }


        private static void SpawnPrefabByGuid(string prefabGuid)
        {
            // Get the prefab's asset path from its GUID
            string prefabPath = AssetDatabase.GUIDToAssetPath(prefabGuid);
            if (string.IsNullOrEmpty(prefabPath))
            {
                Debug.LogError($"No asset found with GUID: {prefabGuid}");
                return;
            }

            // Load the prefab
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null)
            {
                Debug.LogError($"Prefab not found, try rebuilding the library Library");
                return;
            }

            // Instantiate in the scene
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            instance.transform.position = Vector3.zero;

            // Register undo operation
            Undo.RegisterCreatedObjectUndo(instance, "Spawn Prefab");
            Selection.activeObject = instance;
        }
    }
}