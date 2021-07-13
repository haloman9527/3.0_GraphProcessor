#region 注 释
/***
 *
 *  Title:
 *  
 *  Description:
 *  
 *  Date:
 *  Version:
 *  Writer: 
 *
 */
#endregion
using CZToolKit.Core.Editors;
using CZToolKit.MVVM;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CZToolKit.GraphProcessor.Editors
{
    [CustomObjectEditor(typeof(BaseGraphElement))]
    public class BaseNodeViewModelObjectEditor : ObjectEditor
    {
        public static HashSet<string> IgnoreProperty = new HashSet<string>() {
            nameof(BaseNode.Title),
            nameof(BaseNode.TitleTint),
            nameof(BaseNode.Tooltip),
            nameof(BaseNode.Icon),
            nameof(BaseNode.IconSize),
            nameof(BaseNode.Expanded)
        };

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();

            BaseGraphElement graphElement = Target as BaseGraphElement;
            foreach (var property in graphElement.BindableProperties)
            {
                if (IgnoreProperty.Contains(property.Key)) continue;

                property.Value.ValueBoxed = EditorGUILayoutExtension.DrawField(property.Key, property.Value.ValueType, property.Value.ValueBoxed);
            }

            if (EditorGUI.EndChangeCheck())
            {

            }
        }
    }
}
