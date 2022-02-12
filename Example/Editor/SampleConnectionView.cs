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
using CZToolKit.GraphProcessor.Editors;
using UnityEditor.Experimental.GraphView;

public class SampleConnectionView : BaseConnectionView
{
    protected override EdgeControl CreateEdgeControl()
    {
        return new EdgeControlA(this)
        {
            capRadius = 4f,
            interceptWidth = 6f
        };
    }
}