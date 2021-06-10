using CZToolKit.Core.Editors;
using UnityEditor;
using UnityEngine;

namespace CZToolKit.GraphProcessor.Editors
{
    [CustomEditor(typeof(BaseGraphAsset), true)]
    public class BaseGraphAssetInspector : BasicEditor
    {
        protected override void RegisterDrawers()
        {
            base.RegisterDrawers();
            RegisterDrawer("serializedGraph", DrawSerializedGraph);
            RegisterDrawer("variablesUnityReference", property =>
            {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.PropertyField(property);
                EditorGUI.EndDisabledGroup();
            });
        }

        private void DrawSerializedGraph(SerializedProperty property)
        {
            EditorGUIExtension.SetFoldoutBool("SerializedGraphPreview",
                EditorGUILayout.BeginFoldoutHeaderGroup(EditorGUIExtension.GetFoldoutBool("SerializedGraphPreview", false), property.displayName));
            if (EditorGUIExtension.GetFoldoutBool("SerializedGraphPreview"))
                GUILayout.TextArea(property.stringValue, EditorStyles.wordWrappedLabel);
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(new GUIContent("Flush", "清理空数据(空节点，空连接等)"), GUILayout.Height(30)))
            {
                (target as BaseGraphAsset).Graph.Flush();
                EditorUtility.SetDirty(target);
            }
            if (GUILayout.Button("Open", GUILayout.Height(30)))
            {
                BaseGraphWindow.OpenGraphAsset(target as BaseGraphAsset);
            }
            EditorGUILayout.EndHorizontal();
        }
    }
}
