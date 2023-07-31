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
using CZToolKit.Common.IMGUI;
using Sirenix.OdinInspector.Editor;
using UnityEditor;

namespace CZToolKit.GraphProcessor.Editors
{
    [CustomObjectEditor(typeof(BaseNodeView))]
    public class BaseNodeInspector : ObjectEditor
    {
        PropertyTree propertyTree;

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
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.IntField(EditorGUIUtility.TrTextContent("ID"), view.ViewModel.ID);
            EditorGUILayout.Vector2IntField(EditorGUIUtility.TrTextContent("Position"), view.ViewModel.Position.ToVector2Int());
            EditorGUI.EndDisabledGroup();
            
            propertyTree.BeginDraw(false);
            foreach (var property in propertyTree.EnumerateTree(false, true))
            {
                switch (property.Name)
                {
                    case nameof(BaseNode.id):
                    case nameof(BaseNode.position):
                        continue;
                }
                EditorGUI.BeginChangeCheck();
                property.Draw();
                if (EditorGUI.EndChangeCheck() && view.ViewModel.TryGetValue(property.Name, out var bindableProperty))
                    bindableProperty.SetValueWithNotify(property.ValueEntry.WeakSmartValue);
            }

            propertyTree.EndDraw();
            Editor.Repaint();
        }

        public override void OnDisable()
        {
            base.OnDisable();
            propertyTree?.Dispose();
        }
    }
}
#endif