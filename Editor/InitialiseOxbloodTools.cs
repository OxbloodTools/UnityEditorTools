using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Oxblood.editor
{
    //i need to clone the image scene and create some directories for the thumbnails to go.  Then, i need to make sure it all still works here and in a fresh project.
    public static class InitialiseOxbloodTools
    {
        public static void Initialise()
        {
            if (!Directory.Exists(StaticPaths.OxbloodGeneratedData))
            {
                Debug.Log("Creating Oxblood Generated Data Directory");
                Directory.CreateDirectory(StaticPaths.OxbloodGeneratedData);
            }

            if (!File.Exists(StaticPaths.PhotoScenePath))
            {
                Debug.Log("Creating Photo Scene File");
                File.Copy(StaticPaths.PhotoSceneResource, StaticPaths.PhotoScenePath, overwrite: true);
            }

            AssetDatabase.Refresh();
        }
    }
}