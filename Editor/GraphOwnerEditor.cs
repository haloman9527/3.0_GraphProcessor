using CZToolKit.Core.Editors;
using System;
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
            RegisterDrawer("unityReference", property =>
            {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.PropertyField(property);
                EditorGUI.EndDisabledGroup();
            });
            RegisterDrawer("graphAsset", property =>
            {
                EditorGUILayout.BeginHorizontal();
                GraphOwner owner = target as GraphOwner;
                owner.GraphAsset = EditorGUILayout.ObjectField(graphContent, (target as GraphOwner).GraphAsset, owner.GraphAssetType, false) as BaseGraphAsset;
                if (GUILayout.Button("Open", GUILayout.Width(50)))
                    BaseGraphWindow.Open(target as GraphOwner);
                EditorGUILayout.EndHorizontal();
            });
        }

        private void DrawSerialziedVaraibles(SerializedProperty property)
        {
            EditorGUIExtension.SetFoldoutBool("SerializedVariablesPreview",
                EditorGUILayout.BeginFoldoutHeaderGroup(EditorGUIExtension.GetFoldoutBool("SerializedVariablesPreview", false), property.displayName));
            if (EditorGUIExtension.GetFoldoutBool("SerializedVariablesPreview"))
                GUILayout.TextArea(property.stringValue, EditorStyles.wordWrappedLabel);
            EditorGUILayout.EndFoldoutHeaderGroup();
        }
    }
}
