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
using CZToolKit.Core.Editors;
using UnityEditor;
using UnityEngine;

namespace CZToolKit.GraphProcessor.Editors
{
    [CustomEditor(typeof(GraphOwner), true)]
    public class GraphOwnerEditor : BasicEditor
    {
        GUIContent graphContent;
        protected override void OnEnable()
        {
            base.OnEnable();
            graphContent = new GUIContent("Graph");
        }

        protected override void RegisterDrawers()
        {
            base.RegisterDrawers();
            RegisterDrawer("serializedVariables", DrawSerialziedVaraibles);
            RegisterDrawer("variablesUnityReference", property =>
            {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.PropertyField(property);
                EditorGUI.EndDisabledGroup();
            });
            RegisterDrawer("serializedGraph", DrawSerialziedVaraibles);
            RegisterDrawer("graphUnityReferences", property =>
            {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.PropertyField(property);
                EditorGUI.EndDisabledGroup();
            });
        }

        public class A
        {
            public int i;
        }
        A a = new A();
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button("Edit", GUILayout.Height(30)))
                BaseGraphWindow.LoadGraphFromAsset(target as IGraphAsset);
        }

        private void DrawSerialziedVaraibles(SerializedProperty property)
        {
            GUIHelper.SetFoldoutBool(property.displayName,
                EditorGUILayout.BeginFoldoutHeaderGroup(GUIHelper.GetFoldoutBool(property.displayName, false), property.displayName));
            if (GUIHelper.GetFoldoutBool(property.displayName))
                GUILayout.TextArea(property.stringValue, EditorStyles.wordWrappedLabel);
            EditorGUILayout.EndFoldoutHeaderGroup();
        }
    }
}
