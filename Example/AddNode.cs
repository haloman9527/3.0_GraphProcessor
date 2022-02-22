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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddNode : BaseNode
{
    protected override void OnEnabled()
    {
        base.OnEnabled();

        AddPort(new BasePort("InputA", BasePort.Orientation.Horizontal, BasePort.Direction.Input, BasePort.Capacity.Single, typeof(float)));
        AddPort(new BasePort("InputB", BasePort.Orientation.Horizontal, BasePort.Direction.Input, BasePort.Capacity.Single, typeof(float)));
        AddPort(new BasePort("Output", BasePort.Orientation.Horizontal, BasePort.Direction.Output, BasePort.Capacity.Multi, typeof(float)));
    }

    public override object GetValue(string port)
    {
        var inputAValue = (float)Ports["InputA"].GetConnectionValue();
        var inputBValue = (float)Ports["InputB"].GetConnectionValue();
        return inputAValue + inputBValue;
    }
}
