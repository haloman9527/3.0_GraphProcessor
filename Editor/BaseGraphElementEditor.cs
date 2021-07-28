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
using CZToolKit.Core.Editors;
using System.Collections.Generic;
using UnityEditor;

namespace CZToolKit.GraphProcessor.Editors
{
    [CustomObjectEditor(typeof(BaseGraphElement))]
    public class BaseNodeObjectEditor : ObjectEditor
    {
        public static HashSet<string> IgnoreProperty = new HashSet<string>() {
            nameof(BaseNode.Title),
            nameof(BaseNode.TitleColor),
            nameof(BaseNode.Tooltip),
            nameof(BaseNode.Icon),
            nameof(BaseNode.IconSize),
            nameof(BaseNode.Expanded)
        };

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();

            if (Target is BaseGraphElement graphElement)
            {
                foreach (var property in graphElement)
                {
                    if (IgnoreProperty.Contains(property.Key)) continue;

                    property.Value.ValueBoxed = EditorGUILayoutExtension.DrawField(GraphProcessorEditorUtility.GetDisplayName(property.Key), property.Value.ValueType, property.Value.ValueBoxed);
                }
            }

            if (EditorGUI.EndChangeCheck())
            {

            }
        }
    }
}
