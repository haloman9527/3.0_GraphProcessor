#if UNITY_EDITOR
using Jiange.GraphProcessor.Editors;
using JiangeEditor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

[CustomView(typeof(FlowNode))]
public class FlowNodeView : BaseNodeView
{
    protected override void OnInitialized()
    {
        base.OnInitialized();

        var editor = ObjectInspectorEditor.CreateEditor(this);
        controls.Add(new IMGUIContainer(() =>
        {
            EditorGUIUtility.labelWidth = 70;
            editor.OnInspectorGUI();
            this.MarkDirtyRepaint();
        }));
    }
}
#endif