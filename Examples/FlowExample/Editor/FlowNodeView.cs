#if UNITY_EDITOR
using CZToolKit.GraphProcessor.Editors;
using CZToolKitEditor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

[CustomView(typeof(FlowNode))]
public class FlowNodeView : BaseNodeView
{
    protected override void OnInitialized()
    {
        base.OnInitialized();

        if (ViewModel.Properties.Count > 3)
        {
            var editor = ObjectInspectorEditor.CreateEditor(this);
            controls.Add(new IMGUIContainer(() =>
            {
                if (this.Owner.worldBound.Overlaps( this.worldBound))
                {
                    EditorGUIUtility.labelWidth = 70;
                    editor.OnInspectorGUI();
                    MarkDirtyRepaint();
                }
            }));
        }
    }
}
#endif