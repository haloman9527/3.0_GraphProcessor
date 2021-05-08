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
        [DidReloadScripts]
        private static void OnScriptsReloaded()
        {
            string[] guids = AssetDatabase.FindAssets($"t:{nameof(BaseGraph)}");
            foreach (var guid in guids)
            {
                BaseGraph graph = AssetDatabase.LoadAssetAtPath<BaseGraph>(AssetDatabase.GUIDToAssetPath(guid));
                if (graph == null) continue;
                graph.CollectionVariables();
                //graph.Variables.Clear();
                //List<SharedVariable> variables = new List<SharedVariable>();
                //Type t = typeof(SharedVariable);
                //foreach (var node in graph.Nodes)
                //{
                //    // 收集所有节点的SharedVariable
                //    List<FieldInfo> fieldInfos = Utility.GetFieldInfos(node.Value.GetType());
                //    foreach (var fieldInfo in fieldInfos)
                //    {
                //        if (!t.IsAssignableFrom(fieldInfo.FieldType)) continue;
                //        SharedVariable variable = fieldInfo.GetValue(node.Value) as SharedVariable;
                //        if (variable == null)
                //        {
                //            variable = Activator.CreateInstance(fieldInfo.FieldType) as SharedVariable;
                //            fieldInfo.SetValue(node.Value, variable);
                //        }
                //        variable.Name = fieldInfo.Name;
                //        if (!graph.Variables.Contains(variable))
                //            graph.Variables.Add(variable);
                //    }
                //}
            }
        }
    }
}
