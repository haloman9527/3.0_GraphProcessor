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
            var view = Target as BaseNodeView;
            if (view.Model != null)
                propertyTree = PropertyTree.Create(view.Model);
        }

        public override void OnInspectorGUI()
        {
            if (propertyTree == null)
                return;
            var view = Target as BaseNodeView;
            var model = view.Model;
            propertyTree.BeginDraw(false);
            foreach (var property in propertyTree.EnumerateTree(false, true))
            {
                EditorGUI.BeginChangeCheck();
                property.Draw();
                if (EditorGUI.EndChangeCheck() && model.TryGetValue(property.Name, out var bindableProperty))
                {
                    bindableProperty.SetValueWithNotify(property.ValueEntry.WeakSmartValue);
                }
            }
            propertyTree.EndDraw();
        }
    }
}
#endif