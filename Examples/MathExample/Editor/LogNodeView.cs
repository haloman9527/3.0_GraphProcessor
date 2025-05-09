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
 *  Github: https://github.com/haloman9527
 *  Blog: https://www.haloman.net/
 *
 */
#endregion

#if UNITY_EDITOR
using Atom.GraphProcessor;
using Atom.GraphProcessor.Editors;
using UnityEngine.UIElements;

[CustomView(typeof(LogNode))]
public class LogNodeView : BaseNodeView
{
    Button btnDebug;

    public LogNodeView() : base()
    {
        btnDebug = new Button();
        btnDebug.text = "Log";
        btnDebug.clicked += OnClick;
        this.controls.Add(btnDebug);
    }

    protected override void DoInit()
    {
        base.DoInit();
        PortViews[ConstValues.FLOW_IN_PORT_NAME].PortLabel.AddToClassList("hidden");
    }

    private void OnClick()
    {
        (ViewModel as LogNodeProcessor).DebugInput();
    }
}
#endif