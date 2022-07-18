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
#if UNITY_EDITOR
using CZToolKit.GraphProcessor.Editors;
using UnityEngine.UIElements;

[CustomView(typeof(DebugNode))]
public class DebugNodeView : BaseNodeView
{
    Button btnDebug;

    public DebugNodeView()
    {
        btnDebug = new Button();
        btnDebug.text = "Debug";
        controlsContainer.Add(btnDebug);
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        btnDebug.clicked += OnClick;
    }

    private void OnClick()
    {
        (ViewModel as DebugNodeVM).DebugInput();
    }
}
#endif