using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

namespace Oxblood.editor
{
    /// <summary>
    /// This is probably not the best practice, but due to all the dependancies making this static class off to one side seems to make sense to me.
    /// </summary>
    public static class StaticData
    {
        public const string TargetLabel = "OxbloodAsset";
        public const string OxbloodGeneratedData = "Assets/zResources/Oxblood/";
        public const string PhotoScenePath = OxbloodGeneratedData + "ThumbnailPhotoStudio.unity";
        public const string UiComponentsPath = "Packages/com.oxblood.oxbloodtools/UI/";
    }
}