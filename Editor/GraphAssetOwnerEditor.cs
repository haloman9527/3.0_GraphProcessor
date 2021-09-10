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
                    BaseGraphWindow.LoadGraphFromAssetOwner(target as GraphAssetOwner);
                EditorGUILayout.EndHorizontal();
            });
        }

        private void DrawSerialziedVaraibles(SerializedProperty property)
        {
            GUIHelper.CacheBool("SerializedVariablesPreview",
                EditorGUILayout.BeginFoldoutHeaderGroup(GUIHelper.GetCachedBool("SerializedVariablesPreview", false), property.displayName));
            if (GUIHelper.GetCachedBool("SerializedVariablesPreview"))
                GUILayout.TextArea(property.stringValue, EditorStyles.wordWrappedLabel);
            EditorGUILayout.EndFoldoutHeaderGroup();
        }
    }
}
