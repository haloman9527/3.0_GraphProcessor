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
using CZToolKit;
using CZToolKit.GraphProcessor;
using UnityEngine;

[NodeMenu("Log")]
public class LogNode : BaseNode { }

[ViewModel(typeof(LogNode))]
public class LogNodeProcessor : BaseNodeProcessor
{
    public LogNodeProcessor(BaseNode model) : base(model)
    {
        AddPort(new BasePortProcessor("Input", BasePort.Orientation.Horizontal, BasePort.Direction.Left, BasePort.Capacity.Single));
    }

    public void DebugInput()
    {
        Debug.Log(Ports["Input"].GetConnectionValue());
    }
}
