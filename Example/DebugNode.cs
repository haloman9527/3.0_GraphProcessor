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
using CZToolKit.GraphProcessor;
using UnityEngine;

public class DebugNode : BaseNode
{
    protected override void OnEnabled()
    {
        base.OnEnabled();
        AddPort(new BasePort("Input", BasePort.Orientation.Horizontal, BasePort.Direction.Input, BasePort.Capacity.Single));
    }

    public void DebugInput()
    {
        Debug.Log(Ports["Input"].GetConnectionValue());
    }
}
