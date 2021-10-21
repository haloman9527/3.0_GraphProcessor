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
using CZToolKit.GraphProcessor.Internal;
using UnityEditor;
using UnityEngine;

namespace CZToolKit.GraphProcessor.Editors
{
    [CustomEditor(typeof(InternalGraphAssetOwner), true)]
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
                InternalGraphAssetOwner owner = target as InternalGraphAssetOwner;
                owner.GraphAsset = EditorGUILayout.ObjectField(graphContent, (target as InternalGraphAssetOwner).GraphAsset, owner.GraphAssetType, false) as InternalBaseGraphAsset;
                if (GUILayout.Button("Edit", GUILayout.Width(50)))
                    BaseGraphWindow.Open(target as InternalGraphAssetOwner);
                EditorGUILayout.EndHorizontal();
            });
        }

        private void DrawSerialziedVaraibles(SerializedProperty property)
        {
            var @bool = GUIHelper.GetContextData("SerializedVariablesPreview", false);
            @bool.value = EditorGUILayout.BeginFoldoutHeaderGroup(@bool.value, property.displayName);
            if (@bool.value)
                GUILayout.TextArea(property.stringValue, EditorStyles.wordWrappedLabel);
            EditorGUILayout.EndFoldoutHeaderGroup();
        }
    }
}
