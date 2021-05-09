using CZToolKit.Core;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System;
using System.Reflection;
using System.Collections.Generic;

namespace CZToolKit.GraphProcessor.Editors
{
    public partial class EditorCallbacks
    {
        //[DidReloadScripts]
        //private static void OnScriptsReloaded()
        //{
        //    string[] guids = AssetDatabase.FindAssets($"t:{nameof(BaseGraph)}");
        //    foreach (var guid in guids)
        //    {
        //        BaseGraph graph = AssetDatabase.LoadAssetAtPath<BaseGraph>(AssetDatabase.GUIDToAssetPath(guid));
        //        if (graph == null) continue;
        //        graph.CollectionVariables();
        //    }
        //}
    }
}
