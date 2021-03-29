using UnityEditor;
using UnityEngine;

#if ODIN_INSPECTOR
using Sirenix.OdinInspector.Editor;
#endif

namespace GraphProcessor.Editors
{
    [CustomEditor(typeof(BaseGraph), true)]
    public class BaseGraphInspector : Editor
    {
#if ODIN_INSPECTOR
        PropertyTree propertyTree;
#endif
        private void OnEnable()
        {
#if ODIN_INSPECTOR
            propertyTree = PropertyTree.Create(target);
            propertyTree.DrawMonoScriptObjectField = true;
#endif
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"));
            EditorGUI.EndDisabledGroup();
#if ODIN_INSPECTOR
            if (propertyTree != null)
            {
#if ODIN_INSPECTOR_3
                propertyTree.BeginDraw(false);
                propertyTree.Draw();
                propertyTree.EndDraw();
#else
                Sirenix.OdinInspector.Editor.InspectorUtilities.BeginDrawPropertyTree(propertyTree, true);
                propertyTree.Draw();
                Sirenix.OdinInspector.Editor.InspectorUtilities.EndDrawPropertyTree(propertyTree);
#endif
            }

#else
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
#endif
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(new GUIContent("Clean","清理空数据(空节点，空连接等)"),GUILayout.Height(30)))
            {
                //Undo.RegisterCompleteObjectUndo(target, "Clean");
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
