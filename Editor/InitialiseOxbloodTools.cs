﻿using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

//using UnityEngine.SceneManagement;

namespace Oxblood.editor
{
    //i need to clone the image scene and create some directories for the thumbnails to go.  Then, i need to make sure it all still works here and in a fresh project.
    public static class InitialiseOxbloodTools
    {
        public static void Initialise()
        {
            if (!Directory.Exists(StaticData.OxbloodGeneratedData))
            {
                Debug.Log("Creating Oxblood Generated Data Directory");
                Directory.CreateDirectory(StaticData.OxbloodGeneratedData);
            }

            if (!File.Exists(StaticData.PhotoScenePath))
            {
                Debug.Log("Creating Oxblood Dependencies");
                string currentScenePath = SceneManager.GetActiveScene().path;
                EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene()); 
                //File.Copy(StaticData.PhotoSceneResource, StaticData.PhotoScenePath, overwrite: true);  would need to be a specific Unity version ?
                
                //Create a scene from scratch to make it version-proof
                Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                
                // Create a Camera
                GameObject cameraObject = new GameObject("Main Camera");
                Camera camera = cameraObject.AddComponent<Camera>();
                camera.tag = "MainCamera";
                cameraObject.transform.position = new Vector3(-10, 10, -10);
                camera.transform.rotation = Quaternion.Euler(35, 45, 0);
                camera.orthographic = true;
                camera.backgroundColor = Color.clear;
                camera.clearFlags = CameraClearFlags.SolidColor;

                // Create a Light
                GameObject lightObject = new GameObject("Main Light");
                Light light = lightObject.AddComponent<Light>();
                light.type = LightType.Directional;
                light.intensity = 3;
                lightObject.transform.rotation = Quaternion.Euler(60, 75, 135);
                
                //Create another light
                GameObject lightObject2 = new GameObject("Main Light2");
                Light light2 = lightObject2.AddComponent<Light>();
                light2.type = LightType.Directional;
                light2.intensity = 1;
                lightObject2.transform.rotation = Quaternion.Euler(-60, 0, 135);
                
                //save it
                EditorSceneManager.SaveScene(scene, StaticData.PhotoScenePath);
                AssetDatabase.ImportAsset(StaticData.PhotoScenePath);
                //Open original scene
                EditorSceneManager.OpenScene(currentScenePath, OpenSceneMode.Single);
                
                Debug.Log("Oxblood Dependencies Generated");
            }
            AssetDatabase.Refresh();
        }
    }
}