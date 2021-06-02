using CZToolKit.Core.Editors;
using System;
using UnityEditor;
using UnityEngine;

namespace CZToolKit.GraphProcessor.Editors
{
    [CustomEditor(typeof(GraphAssetOwner), true)]
    public class GraphAssetOwnerEditor : BasicEditor
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

            RegisterDrawer("graphAsset", property =>
            {
                EditorGUILayout.BeginHorizontal();
                GraphAssetOwner owner = target as GraphAssetOwner;
                owner.GraphAsset = EditorGUILayout.ObjectField(graphContent, (target as GraphAssetOwner).GraphAsset, owner.GraphAssetType, false) as BaseGraphAsset;
                if (GUILayout.Button("Edit", GUILayout.Width(50)))
                    BaseGraphWindow.Open(target as GraphAssetOwner);
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
