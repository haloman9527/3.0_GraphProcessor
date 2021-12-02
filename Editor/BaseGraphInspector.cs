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

            EditorGUILayoutExtension.BeginVerticalBoxGroup();
            GUILayout.Label("Graph", bigLabel.value);
            EditorGUILayoutExtension.EndVerticalBoxGroup();

            if (Target is BaseGraphView view && view.Model != null)
            {
                EditorGUILayoutExtension.BeginVerticalBoxGroup();
                GUILayout.Label(string.Concat("Nodes：", view.Model.Nodes.Count), bigLabel.value);
                GUILayout.Label(string.Concat("Connections：", view.Model.Connections.Count), bigLabel.value);
                EditorGUILayoutExtension.EndVerticalBoxGroup();

                EditorGUI.BeginChangeCheck();
                EditorGUILayoutExtension.BeginVerticalBoxGroup();
                foreach (var property in view.Model)
                {
                    if (IgnoreProperties.Contains(property.Key)) continue;

                    object newValue = EditorGUILayoutExtension.DrawField(property.Value.ValueBoxed, GraphProcessorEditorUtility.GetDisplayName(property.Key));
                    if (newValue == null || !newValue.Equals(property.Value.ValueBoxed))
                        property.Value.ValueBoxed = newValue;

                }
                EditorGUILayoutExtension.EndVerticalBoxGroup();
                if (EditorGUI.EndChangeCheck())
                {

                }
            }
        }
    }
}
#endif