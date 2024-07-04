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
 *  Github: https://github.com/haloman9527
 *  Blog: https://www.haloman.net/
 *
 */

#endregion

#if UNITY_EDITOR && ODIN_INSPECTOR
using System;
using System.Collections.Generic;
using CZToolKit;
using CZToolKitEditor;
using Sirenix.OdinInspector.Editor;
using UnityEditor;

namespace CZToolKit.GraphProcessor.Editors
{
    [CustomObjectEditor(typeof(BaseNodeView))]
    public class BaseNodeInspector : ObjectEditor
    {
        private static Dictionary<string, Action<InspectorProperty>> s_CustomDrawers = new Dictionary<string, Action<InspectorProperty>>();

        static BaseNodeInspector()
        {
            s_CustomDrawers[nameof(BaseNode.id)] = CustomIDDrawer;
        }

        private static void CustomIDDrawer(InspectorProperty property)
        {
            // EditorGUILayout.IntField(EditorGUIUtility.TrTextContent("ID"), view.ViewModel.ID);
        }

        private static void CustomPositionDrawer(InspectorProperty property)
        {
            // EditorGUILayout.Vector2IntField(EditorGUIUtility.TrTextContent("Position"), view.ViewModel.Position.ToVector2Int());
        }

        private PropertyTree propertyTree;

        public override void OnEnable()
        {
            if (Target == null)
            {
                base.OnEnable();
                return;
            }

            var view = Target as BaseNodeView;
            if (view == null || view.ViewModel == null)
                return;

            propertyTree = PropertyTree.Create(view.ViewModel.Model);
            propertyTree.DrawMonoScriptObjectField = true;
        }

        public override void OnInspectorGUI()
        {
            var view = Target as BaseNodeView;
            if (view == null || view.ViewModel == null)
                return;
            if (propertyTree == null)
                return;

            propertyTree.BeginDraw(false);
            foreach (var property in propertyTree.EnumerateTree(false, true))
            {
                switch (property.Path)
                {
                    case nameof(BaseNode.id):
                    case nameof(BaseNode.position):
                        continue;
                }

                EditorGUI.BeginChangeCheck();
                property.Draw();
                if (EditorGUI.EndChangeCheck() && view.ViewModel.TryGetProperty(property.Name, out var bindableProperty))
                    bindableProperty.SetValueWithNotify(property.ValueEntry.WeakSmartValue);
            }

            propertyTree.EndDraw();
            Editor?.Repaint();
        }

        public override void OnDisable()
        {
            base.OnDisable();
            propertyTree?.Dispose();
        }
    }
}
#endif