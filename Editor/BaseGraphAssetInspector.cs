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
#if UNITY_EDITOR
using CZToolKit.Core.Editors;
using CZToolKit.GraphProcessor.Internal;
using System;
using UnityEditor;
using UnityEngine;

namespace CZToolKit.GraphProcessor.Editors
{
    [CustomEditor(typeof(InternalBaseGraphAsset), true)]
    public class BaseGraphAssetInspector : BasicEditor
    {
        static GUIHelper.ContextDataCache ContextDataCache = new GUIHelper.ContextDataCache();

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
            var @bool = GUIHelper.GetContextData("SerializedGraphPreview", false);
            @bool.value = EditorGUILayout.BeginFoldoutHeaderGroup(@bool.value, property.displayName);
            if (@bool.value)
                GUILayout.TextArea(property.stringValue, EditorStyles.wordWrappedLabel);
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (!ContextDataCache.TryGetContextData<GUIStyle>("BigLabel", out var bigLabel))
            {
                bigLabel.value = new GUIStyle(GUI.skin.label);
                bigLabel.value.fontSize = 18;
                bigLabel.value.fontStyle = FontStyle.Bold;
                bigLabel.value.alignment = TextAnchor.MiddleLeft;
                bigLabel.value.stretchWidth = true;
            }

            IGraphAsset graphAsset = target as IGraphAsset;

            EditorGUILayoutExtension.BeginVerticalBoxGroup();
            //GUILayout.Label(string.Concat("Nodes：", graphAsset.Graph.Nodes.Count), bigLabel.value);
            //GUILayout.Label(string.Concat("Connections：", graphAsset.Graph.Connections.Count), bigLabel.value);
            EditorGUILayoutExtension.EndVerticalBoxGroup();

            if (GUILayout.Button("Open", GUILayout.Height(30)))
                BaseGraphWindow.Open(target as InternalBaseGraphAsset);
        }
    }
}
#endif