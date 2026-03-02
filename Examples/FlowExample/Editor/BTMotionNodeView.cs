#if UNITY_EDITOR
using Atom.GraphProcessor.Editors;
using Atom.UnityEditors;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

[CustomView(typeof(BTMotionNode))]
public class BTMotionNodeView : BaseNodeView
{
    private PropertyTree editor;

    public BTMotionNodeView()
    {
        controls.Add(new IMGUIContainer(DrawEditor));
    }
    
    protected override void DoInit()
    {
        
    }

    private void DrawEditor()
    {
        if (editor == null)
            editor = PropertyTree.Create(this.ViewModel.Model);

        editor.BeginDraw(false);
        foreach (var child in editor.RootProperty.Children)
        {
            child.Draw();
        }
        editor.EndDraw();
    }
}
#endif