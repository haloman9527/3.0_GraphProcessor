using CZToolKit.Core.Editors;
using System;
using UnityEditor;
using UnityEngine;

namespace CZToolKit.GraphProcessor.Editors
{
    [CustomPropertyDrawer(typeof(SharedVariableAttribute))]
    public class SharedVariableDrawer : PropertyDrawer
    {
        BaseGraphWindow window;
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.serializedObject.targetObject is GraphOwner)
            {
                EditorGUI.PropertyField(position, property.FindPropertyRelative("value"), label, true);
                return;
            }

            NodeInspectorObject ins = Selection.activeObject as NodeInspectorObject;
            if (ins == null) { EditorGUI.HelpBox(position, "不支持的位置", MessageType.Error); return; }
            if (window == null)
                window = BaseGraphWindow.GetWindow(ins.Node.Owner);
            IGraphOwner graphOwner = null;
            if (window == null || (graphOwner = window.GraphOwner) == null) { EditorGUI.HelpBox(position, "找不到GraphOwner", MessageType.Error); return; }

            string guid = property.FindPropertyRelative("guid").stringValue;
            EditorGUI.HelpBox(position, "ReferenceType:" + guid, MessageType.Info);


            //SharedVariable variable = graphOwner.GetVariable(guid);
            //if (variable == null)
            //{
            //    return;
            //    //给Owner创建一个新的
            //    Type fieldType = Type.GetType(property.type);
            //    if (fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == typeof(Nullable<>))
            //        fieldType = Nullable.GetUnderlyingType(fieldType);
            //    variable = Activator.CreateInstance(fieldType, guid) as SharedVariable;
            //    graphOwner.SetVariable(variable.GUID, variable);
            //}

            //EditorGUILayoutExtension.DrawField(label, variable.GetType(), variable);
            Debug.Log(fieldInfo == null);
            EditorGUILayout.PropertyField(property.FindPropertyRelative("value"), label, true);
        }
    }
}
