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
using UnityEditor;
using UnityEngine;

namespace CZToolKit.GraphProcessor.Editors
{
    [CustomFieldDrawer(typeof(SharedVariableAttribute))]
    public class SharedVariableObjectDrawer : FieldDrawer
    {
        BaseGraphWindow window;
        public override void OnGUI(GUIContent label)
        {
            ObjectInspector ins = Selection.activeObject as ObjectInspector;
            if (ins == null) { EditorGUILayout.HelpBox("不支持的位置", MessageType.Error); return; }
            BaseNode node = ins.targetObject as BaseNode;
            if (node != null && window == null)
                window = BaseGraphWindow.GetWindow(node.Owner);
            IGraphOwner graphOwner = null;
            if (window == null || (graphOwner = window.GraphOwner) == null) { EditorGUILayout.HelpBox("找不到GraphOwner", MessageType.Error); return; }
            SharedVariable variable = Value as SharedVariable;
            EditorGUILayout.HelpBox("ReferenceType:" + variable.GUID, MessageType.Info);
            variable.SetValue(EditorGUILayoutExtension.DrawField(label, variable.GetValueType(), variable.GetValue()));
            if (GUI.changed)
            {
                EditorUtility.SetDirty(graphOwner.GetObject());
            }
        }
    }
}
