#region 注 释
/***
 *
 *  Title:
 *  
 *  Description:
 *  
 *  Date:
 *  Version:
 *  Writer: 半只龙虾人
 *  Github: https://github.com/HalfLobsterMan
 *  Blog: https://www.crosshair.top/
 *
 */
#endregion
#if UNITY_EDITOR
using CZToolKit.Core.Editors;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CZToolKit.GraphProcessor.Editors
{
    [CustomObjectEditor(typeof(BaseGraphView))]
    public class BaseGraphInspector : ObjectEditor
    {
        static GUIHelper.ContextDataCache ContextDataCache = new GUIHelper.ContextDataCache();
        static HashSet<string> IgnoreProperties = new HashSet<string>()
        {
            BaseGraph.PAN_NAME,
            BaseGraph.ZOOM_NAME
        };

        public override void OnInspectorGUI()
        {
            if (!ContextDataCache.TryGetContextData<GUIStyle>("BigLabel", out var bigLabel))
            {
                bigLabel.value = new GUIStyle(GUI.skin.label);
                bigLabel.value.fontSize = 18;
                bigLabel.value.fontStyle = FontStyle.Bold;
                bigLabel.value.alignment = TextAnchor.MiddleLeft;
                bigLabel.value.stretchWidth = true;
            }

            EditorGUILayoutExtension.BeginBoxGroup();
            GUILayout.Label("Graph", bigLabel.value);
            EditorGUILayoutExtension.EndBoxGroup();

            if (Target is BaseGraphView view && view.Model != null)
            {
                EditorGUILayoutExtension.BeginBoxGroup();
                GUILayout.Label(string.Concat("Nodes：", view.Model.Nodes.Count), bigLabel.value);
                GUILayout.Label(string.Concat("Connections：", view.Model.Connections.Count), bigLabel.value);
                EditorGUILayoutExtension.EndBoxGroup();

                EditorGUI.BeginChangeCheck();
                EditorGUILayoutExtension.BeginBoxGroup();
                foreach (var property in view.Model)
                {
                    if (IgnoreProperties.Contains(property.Key)) continue;

                    object newValue = EditorGUILayoutExtension.DrawField(GraphProcessorEditorUtility.GetDisplayName(property.Key), property.Value.ValueType, property.Value.ValueBoxed);
                    if (newValue == null || !newValue.Equals(property.Value.ValueBoxed))
                        property.Value.ValueBoxed = newValue;

                }
                EditorGUILayoutExtension.EndBoxGroup();
                if (EditorGUI.EndChangeCheck())
                {

                }
            }
        }
    }
}
#endif