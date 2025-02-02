using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

namespace Oxblood.editor
{
    /// <summary>
    /// So, i need to create the photo scene as a sample because apparently packages are immutable.  This will also mean I need to save the thumbnail somewhere inside 'Assets' for the same reason.
    /// I will clone the scene to an Assets/Oxblood folder, and use the same location for thumbnails.  Obfuscate?  Is it possible to have the editor hide such a folder?
    /// </summary>
    public static class StaticPaths
    {
        public const string TargetLabel = "OxbloodAsset";
        public const string AssetLibraryDataPath = "Packages/com.oxblood.oxbloodtools/Resources/LibraryData.txt";
        public const string PhotoSceneResource = "Packages/com.oxblood.oxbloodtools/Resources/PhotoStudio.unity";
        public const string PhotoScenePath = "Assets/Oxblood/PhotoStudio.unity";
        public const string OxbloodGeneratedData = "Assets/Oxblood/";

    }
}