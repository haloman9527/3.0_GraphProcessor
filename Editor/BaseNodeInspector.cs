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
using UnityEditor;
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
                bigLabel.value = new GUIStyle(GUI.skin.box);
                bigLabel.value.fontSize = 25;
                bigLabel.value.fontStyle = FontStyle.Bold;
                bigLabel.value.stretchWidth = true;
            }
            GUILayout.Box("Node", bigLabel.value);

            if (Target is BaseNodeView view && view.Model != null)
            {
                EditorGUI.BeginChangeCheck();
                Event current = Event.current;
                foreach (var property in view.Model)
                {
                    if (IgnoreProperties.Contains(property.Key)) continue;

                    object newValue = EditorGUILayoutExtension.DrawField(GraphProcessorEditorUtility.GetDisplayName(property.Key), property.Value.ValueType, property.Value.ValueBoxed);
                    if (!newValue.Equals(property.Value.ValueBoxed))
                        property.Value.ValueBoxed = newValue;

                }
                if (EditorGUI.EndChangeCheck())
                {

                }
            }
        }
    }
}
#endif