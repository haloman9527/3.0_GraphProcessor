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
    [CustomObjectDrawer(typeof(SharedVariableAttribute))]
    public class SharedVariableObjectDrawer : ObjectDrawer
    {
        //BaseGraphWindow window;
        public override void OnGUI(GUIContent label)
        {
            //NodeInspectorObject ins = Selection.activeObject as NodeInspectorObject;
            //if (ins == null) { EditorGUILayout.HelpBox("不支持的位置", MessageType.Error); return; }
            //if (window == null)
            //    window = BaseGraphWindow.GetWindow(ins.Node.Owner);
            //IGraphOwner graphOwner = null;
            //if (window == null || (graphOwner = window.GraphOwner) == null) { EditorGUILayout.HelpBox("找不到GraphOwner", MessageType.Error); return; }
            //EditorGUILayout.HelpBox("ReferenceType", MessageType.Info);
            SharedVariable variable = Value as SharedVariable;
            variable.SetValue(EditorGUILayoutExtension.DrawField(label, variable.GetValueType(), variable.GetValue()));
        }
    }
}
