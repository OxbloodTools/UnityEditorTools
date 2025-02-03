using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Oxblood.editor
{
    public class AssetGrabber : Editor
    {
        public List<string> oxbloodAssetGuids = new List<string>(); //I want this to be static, possible?

        public void RebuildOxbloodAssetDatabase(bool forceFull)
        {
            string currentScenePath = SceneManager.GetActiveScene().path; //so we can return to scene we started in after screenshots are done
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene()); // ask to save current scene before beginning


            RefreshAssetLibraryGuids(forceFull);
            foreach (string guid in oxbloodAssetGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                TakeObjectPicture(prefab, guid);
            }


            EditorSceneManager.OpenScene(currentScenePath, OpenSceneMode.Single);
        }


        public void RefreshAssetLibraryGuids(bool full) // re-scans the project for labelled assets then populate the main dictionary with Guids and names
        {
            oxbloodAssetGuids.Clear();
            string[] guids = UnityEditor.AssetDatabase.FindAssets($"l:{StaticPaths.TargetLabel}");

            if (!full) //this is disgusting
            {
                string[] existingGuidsPaths = Directory.GetFiles(StaticPaths.OxbloodGeneratedData); //all the objects in the resources folder (images AND all the other junk)

                string searchString = "";
                foreach (string existingPath in existingGuidsPaths)
                {
                    searchString += existingPath;
                }

                for (int i = guids.Length - 1; i >= 0; i--)
                {
                    if (searchString.Contains(guids[i]))
                    {
                        guids[i] = "";
                    }
                }
            }

            foreach (string guid in guids)
            {
                if (guid != "") oxbloodAssetGuids.Add(guid);
            }
        }
       
        #region Methods

        private void TakeObjectPicture(GameObject prefab, string guid)
        {
            Scene scene = EditorSceneManager.OpenScene(StaticPaths.PhotoScenePath, OpenSceneMode.Single);
            GameObject instance = PrefabUtility.InstantiatePrefab(prefab, scene) as GameObject;

            Bounds bounds = GetPrefabBounds(instance);
            float maxExtent = Mathf.Max(bounds.extents.x, bounds.extents.y, bounds.extents.z);

            Camera screenshotCamera = FindFirstObjectByType<Camera>(); //make sure theres always a camera in the test scene!
            screenshotCamera.transform.LookAt(bounds.center);
            screenshotCamera.orthographicSize = maxExtent * 1.2f;


            string fullPath = StaticPaths.OxbloodGeneratedData + guid + ".png";

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


            AssetDatabase.Refresh();
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
    }
}