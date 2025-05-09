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
#if UNITY_EDITOR
using Atom.UnityEditors;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace Atom.GraphProcessor.Editors
{
    [CustomObjectEditor(typeof(BaseConnectionView))]
    public class BaseConnectionInspector : ObjectEditor
    {
        PropertyTree propertyTree;

        public override void OnEnable()
        {
            var view = Target as BaseNodeView;
            if (view == null || view.ViewModel == null)
                return;
            if (view.BindingProperty != null)
            {
            }
            else
            {
                propertyTree = PropertyTree.Create(view.ViewModel.Model);
                propertyTree.DrawMonoScriptObjectField = true;
            }
        }

        public override sealed void OnInspectorGUI()
        {
            var view = Target as BaseConnectionView;
            if (view == null || view.ViewModel == null)
                return;

            if (false && view.BindingProperty != null)
            {
                view.BindingProperty.serializedObject.Update();
                EditorGUILayout.PropertyField(view.BindingProperty, GUIContent.none, true);
                if (view.BindingProperty.serializedObject.hasModifiedProperties)
                {
                    view.BindingProperty.serializedObject.ApplyModifiedProperties();
                }

                SourceEditor?.Repaint();
            }
            else
            {
                if (propertyTree != null)
                {
                    propertyTree.BeginDraw(false);
                    foreach (var property in propertyTree.EnumerateTree(false, true))
                    {
                        switch (property.Name)
                        {
                            case nameof(BaseConnection.fromNode):
                            case nameof(BaseConnection.fromPort):
                            case nameof(BaseConnection.toNode):
                            case nameof(BaseConnection.toPort):
                                continue;
                        }
                        property.Draw();
                    }
                    propertyTree.EndDraw();
                    SourceEditor.Repaint();
                }
            }
        }

        public override void OnDisable()
        {
            base.OnDisable();
            propertyTree?.Dispose();
        }
    }
}
#endif