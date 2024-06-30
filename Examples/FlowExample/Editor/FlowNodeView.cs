#if UNITY_EDITOR
using CZToolKit.GraphProcessor.Editors;
using CZToolKitEditor;
using UnityEditor;
using UnityEngine.UIElements;

[CustomView(typeof(FlowNode))]
public class FlowNodeView : BaseNodeView
{
    protected override void OnInitialized()
    {
        base.OnInitialized();

        if (ViewModel.Count > 3)
        {
            var editor = ObjectEditor.CreateEditor(this);
            editor.OnEnable();
            controls.Add(new IMGUIContainer(() =>
            {
                EditorGUIUtility.labelWidth = 70;
                EditorGUI.BeginChangeCheck();
                editor.OnInspectorGUI();
                MarkDirtyRepaint();
            }));
        }
    }
}
#endif