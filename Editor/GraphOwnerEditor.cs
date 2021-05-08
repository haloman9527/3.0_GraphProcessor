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

            RegisterDrawer("graph", property =>
            {
                EditorGUILayout.BeginHorizontal();
                GraphOwner owner = target as GraphOwner;
                owner.Graph = EditorGUILayout.ObjectField(graphContent, (target as GraphOwner).Graph, owner.GraphType, false) as BaseGraph;
                if (GUILayout.Button("Open", GUILayout.Width(50)))
                    BaseGraphWindow.Open(target as GraphOwner);
                EditorGUILayout.EndHorizontal();
            });
        }
    }
}
