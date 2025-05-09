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
    [CustomObjectEditor(typeof(BaseNodeView))]
    public class BaseNodeInspector : ObjectEditor
    {
        private PropertyTree propertyTree;

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
            var view = Target as BaseNodeView;
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
                        switch (property.Path)
                        {
                            case nameof(BaseNode.id):
                            case nameof(BaseNode.position):
                                continue;
                        }

                        property.Draw();
                    }

                    propertyTree.EndDraw();
                    SourceEditor?.Repaint();
                }
            }
        }

        public override void OnDisable()
        {
            base.OnDisable();
            propertyTree?.Dispose();
        }
    }

    [UnityEditor.CustomPropertyDrawer(typeof(BaseNode))]
    public class BaseNodeDrawer : UnityEditor.PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var end = property.GetEndProperty();
            var enterChildren = true;
            while (property.NextVisible(enterChildren) && !SerializedProperty.EqualContents(property, end))
            {
                enterChildren = false;
                EditorGUILayout.PropertyField(property, true);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return 0;
        }
    }
}
#endif