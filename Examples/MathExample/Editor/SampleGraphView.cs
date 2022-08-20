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
using CZToolKit.GraphProcessor;
using CZToolKit.GraphProcessor.Editors;
using System;
using System.Collections.Generic;

public class SampleGraphView : BaseGraphView
{
    protected override IEnumerable<Type> GetNodeTypes()
    {
        yield return typeof(FloatNode);
        yield return typeof(AddNode);
        yield return typeof(SubNode);
        yield return typeof(DebugNode);
    }

    protected override BaseConnectionView NewConnectionView(BaseConnectionVM connection)
    {
        return new SampleConnectionView();
    }
}
#endif