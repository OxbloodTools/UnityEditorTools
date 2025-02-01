using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Oxblood.editor
{
    public class AssetGrabber : Editor
    {
        private const string TargetLabel = "OxbloodAsset";
        private const string AssetLibraryDataPath = "Packages/com.oxblood.oxbloodtools/Res/LibraryData.txt";
        public readonly string ThumbnailDirectoryPath = "Packages/com.oxblood.oxbloodtools/Res/AssetThumbs/";
        public List<string> oxbloodAssetGuids = new List<string>(); //I want this to be static, possible?

        //I discovered a huge amount of redundancy here as I realised serialisation etc is unnecessary because I can use the generated image files and names as links to prefabs with GUIDS, cunt.

        public void RebuildOxbloodAssetDatabase() //used when doing a complete rescan of the library.  Will wipe everything each time and start fresh - feels like this will be hefty no matter what
        {
            string currentScenePath = SceneManager.GetActiveScene().path; //so we can return to scene we started in after screenshots are done
            string[] existingImgFiles = Directory.GetFiles(ThumbnailDirectoryPath);
            foreach (string existingImgFile in existingImgFiles) File.Delete(existingImgFile);

            RefreshAssetLibraryGuids();
            foreach (string guid in oxbloodAssetGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                TakeObjectPicture(prefab, guid);
            }
           
            EditorSceneManager.OpenScene(currentScenePath, OpenSceneMode.Single);
        }

        public void RefreshAssetLibraryGuids() // re-scans the project for labelled assets then populate the main dictionary with Guids and names  DONT NEED NAMES FOOL!
        {
            oxbloodAssetGuids.Clear();
            string[] guids = UnityEditor.AssetDatabase.FindAssets($"l:{TargetLabel}");
            foreach (string guid in guids)
            {
                oxbloodAssetGuids.Add(guid);
            }
        }

        #region Saving & Loading

        private void SaveDatabaseInfo() //saves the guids gathered to a json file
        {
            OxbloodAssetDatabase database = new OxbloodAssetDatabase();
            database.guids = oxbloodAssetGuids.ToArray();

            string assetDatabaseAsString = JsonUtility.ToJson(database);
            File.WriteAllText(AssetLibraryDataPath, assetDatabaseAsString);
        }

        private void LoadDatabaseInfo() // loads previously caluclated guids from disc
        {
            if (File.Exists(AssetLibraryDataPath))
            {
                string assetDatabaseAsString = File.ReadAllText(AssetLibraryDataPath);
                OxbloodAssetDatabase database = JsonUtility.FromJson<OxbloodAssetDatabase>(assetDatabaseAsString);

                oxbloodAssetGuids.Clear();
                oxbloodAssetGuids.AddRange(database.guids);
            }
            else
            {
                Debug.LogWarning("No OxbloodAsset database found");
            }
        }

        private void ScrapeForExistingAssets() //after loading data, link up existing images with guids and display
        {
        }

        [System.Serializable]
        public class OxbloodAssetDatabase
        {
            public string[] guids;
        }

        #endregion

        #region Methods

        private void TakeObjectPicture(GameObject prefab, string guid)
        {
            Scene scene = EditorSceneManager.OpenScene("Packages/com.oxblood.oxbloodtools/Res/PhotoStudio.unity", OpenSceneMode.Single);
            GameObject instance = PrefabUtility.InstantiatePrefab(prefab, scene) as GameObject;

            Bounds bounds = GetPrefabBounds(instance);
            float maxExtent = Mathf.Max(bounds.extents.x, bounds.extents.y, bounds.extents.z);

            Camera screenshotCamera = FindFirstObjectByType<Camera>(); //make sure theres always a camera in the test scene!
            screenshotCamera.transform.LookAt(bounds.center);
            screenshotCamera.backgroundColor = Color.clear;
            screenshotCamera.orthographicSize = maxExtent * 1.2f;


            string fullPath = ThumbnailDirectoryPath + guid + ".png";

            RenderTexture rt = new RenderTexture(256, 256, 24, RenderTextureFormat.ARGB32);
            screenshotCamera.targetTexture = rt;
            Texture2D screenshot = new Texture2D(256, 256, TextureFormat.ARGB32, false);
            screenshotCamera.Render();
            RenderTexture.active = rt;
            screenshot.ReadPixels(new Rect(0, 0, 256, 256), 0, 0);
            screenshot.Apply();

            byte[] bytes = screenshot.EncodeToPNG();
            File.WriteAllBytes(fullPath, bytes);
            //Debug.Log("Screenshot saved to: " + fullPath);

            RenderTexture.active = null;
            Object.DestroyImmediate(screenshotCamera.gameObject);
            Object.DestroyImmediate(rt);
            Object.DestroyImmediate(instance);


            UnityEditor.AssetDatabase.Refresh();
        }

        private Bounds GetPrefabBounds(GameObject go)
        {
            Renderer[] renderers = go.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0)
            {
                return new Bounds(go.transform.position, Vector3.zero);
            }

            Bounds bounds = new Bounds(renderers[0].bounds.center, renderers[0].bounds.size);
            for (int i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }

            return bounds;
        }

        #endregion

        public AssetGrabber() //default constructor
        {
            //LoadDatabaseInfo();
        }
    }
}