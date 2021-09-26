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
            var @bool = GUIHelper.GetContextData("SerializedGraphPreview", false);
            @bool.value = EditorGUILayout.BeginFoldoutHeaderGroup(@bool.value, property.displayName);
            if (@bool.value)
                GUILayout.TextArea(property.stringValue, EditorStyles.wordWrappedLabel);
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUILayout.BeginHorizontal();
            // *****
            //if (GUILayout.Button(new GUIContent("Flush", "清理空数据(空节点，空连接等)"), GUILayout.Height(30)))
            //{
            //    (target as BaseGraphAsset).Graph.Flush();
            //    EditorUtility.SetDirty(target);
            //}
            if (GUILayout.Button("Open", GUILayout.Height(30)))
            {
                BaseGraphWindow.LoadGraphFromAsset(target as BaseGraphAsset);
            }
            EditorGUILayout.EndHorizontal();
        }
    }
}
