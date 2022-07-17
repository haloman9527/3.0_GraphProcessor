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
#if UNITY_EDITOR && ODIN_INSPECTOR
using CZToolKit.Core.Editors;
using UnityEditor;
using UnityEngine;
using Sirenix.OdinInspector.Editor;

namespace CZToolKit.GraphProcessor.Editors
{
    [CustomObjectEditor(typeof(BaseGraphView))]
    public class BaseGraphInspector : ObjectEditor
    {
        static GUIHelper.ContextDataCache ContextDataCache = new GUIHelper.ContextDataCache();

        PropertyTree propertyTree;

        public override void OnEnable()
        {
            var view = Target as BaseGraphView;
            if (view.ViewModel != null)
                propertyTree = PropertyTree.Create(view.ViewModel.Model);
        }

        public override void OnInspectorGUI()
        {
            var view = Target as BaseGraphView;
            if (view == null || view.ViewModel == null)
                return;

            if (!ContextDataCache.TryGetContextData<GUIStyle>("BigLabel", out var bigLabel))
            {
                bigLabel.value = new GUIStyle(GUI.skin.label);
                bigLabel.value.fontSize = 18;
                bigLabel.value.fontStyle = FontStyle.Bold;
                bigLabel.value.alignment = TextAnchor.MiddleLeft;
                bigLabel.value.stretchWidth = true;
            }

            EditorGUILayoutExtension.BeginVerticalBoxGroup();
            GUILayout.Label(string.Concat("Nodes：", view.ViewModel.Nodes.Count), bigLabel.value);
            GUILayout.Label(string.Concat("Connections：", view.ViewModel.Connections.Count), bigLabel.value);
            EditorGUILayoutExtension.EndVerticalBoxGroup();

            if (propertyTree == null)
                return;
            propertyTree.BeginDraw(false);
            foreach (var property in propertyTree.EnumerateTree(false, true))
            {
                EditorGUI.BeginChangeCheck();
                property.Draw();
                if (EditorGUI.EndChangeCheck() && view.ViewModel.TryGetValue(property.Name, out var bindableProperty))
                    bindableProperty.SetValueWithNotify(property.ValueEntry.WeakSmartValue);
            }
            propertyTree.EndDraw();
        }
    }
}
#endif