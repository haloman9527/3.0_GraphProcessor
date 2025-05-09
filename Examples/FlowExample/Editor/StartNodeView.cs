#if UNITY_EDITOR
using Atom.GraphProcessor.Editors;
using Atom.UnityEditors;
using UnityEditor;
using UnityEngine.UIElements;

[CustomView(typeof(SVNUpdateNode))]
public class SVNUpdateNodeView : BaseNodeView
{
    private Editor editor;

    public SVNUpdateNodeView()
    {
        controls.Add(new IMGUIContainer(DrawEditor));
    }
    
    protected override void DoInit()
    {
        editor = ObjectInspectorEditor.CreateEditor(this);
    }

    private void DrawEditor()
    {
        if (editor == null)
            return;
        
        var wideMode = EditorGUIUtility.wideMode;
        EditorGUIUtility.wideMode = true;
        EditorGUI.BeginChangeCheck();
        editor.OnInspectorGUI();
        if (EditorGUI.EndChangeCheck())
            this.MarkDirtyRepaint();
        EditorGUIUtility.wideMode = wideMode;
    }
}
#endif