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
using System.Collections.Generic;
using UnityEngine;

namespace CZToolKit.GraphProcessor.Editors
{
    [CustomObjectEditor(typeof(BaseNodeView))]
    public class BaseNodeInspector : ObjectEditor
    {
        public static HashSet<string> IgnoreProperties = new HashSet<string>() {
            BaseNode.TITLE_NAME,
            BaseNode.TITLE_COLOR_NAME,
            BaseNode.TOOLTIP_NAME
        };

        static GUIHelper.ContextDataCache ContextDataCache = new GUIHelper.ContextDataCache();

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

            if (Target is BaseNodeView view && view.Model != null)
            {
                EditorGUILayoutExtension.BeginBoxGroup();
                bigLabel.value.alignment = TextAnchor.MiddleLeft;
                GUILayout.Label(string.Concat("Node：", view.Model.GUID), bigLabel.value);
                EditorGUILayoutExtension.EndBoxGroup();

                EditorGUILayoutExtension.BeginBoxGroup();
                foreach (var property in view.Model)
                {
                    if (IgnoreProperties.Contains(property.Key)) continue;

                    object newValue = EditorGUILayoutExtension.DrawField(property.Value.ValueType, property.Value.ValueBoxed, GraphProcessorEditorUtility.GetDisplayName(property.Key));
                    if (newValue == null || !newValue.Equals(property.Value.ValueBoxed))
                        property.Value.ValueBoxed = newValue;

                }
                EditorGUILayoutExtension.EndBoxGroup();
            }
        }
    }
}
#endif