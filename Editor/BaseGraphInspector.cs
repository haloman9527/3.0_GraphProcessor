using UnityEditor;
using UnityEngine;

namespace CZToolKit.GraphProcessor.Editors
{
    [CustomEditor(typeof(BaseGraph), true)]
    public class BaseGraphInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUI.BeginDisabledGroup(true);

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"));

            EditorGUI.BeginChangeCheck();
            SerializedProperty iterator = serializedObject.GetIterator();
            iterator.NextVisible(true);
            do
            {
                if (iterator.name == "m_Script") continue;
                EditorGUILayout.PropertyField(iterator);
            } while (iterator.NextVisible(false));

            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();
            serializedObject.Update();

            EditorGUI.EndDisabledGroup();

            GUILayout.Space(30);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(new GUIContent("Clean","清理空数据(空节点，空连接等)"),GUILayout.Height(30)))
            {
                (target as BaseGraph).Clean();
                EditorUtility.SetDirty(target);
            }
            if (GUILayout.Button("Open", GUILayout.Height(30)))
            {
                BaseGraphWindow.LoadGraph(target as BaseGraph);
            }
            EditorGUILayout.EndHorizontal();
        }
    }
}
