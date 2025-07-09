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
using Atom;
using Atom.GraphProcessor;
using UnityEngine;

[NodeMenu("Log")]
public class LogNode : BaseNode { }

[ViewModel(typeof(LogNode))]
public class LogNodeProcessor : BaseNodeProcessor
{
    public LogNodeProcessor(BaseNode model) : base(model)
    {
        AddPort(new PortProcessor(ConstValues.FLOW_IN_PORT_NAME, BasePort.Direction.Left, BasePort.Capacity.Single));
    }

    public void DebugInput()
    {
        Debug.Log(Ports[ConstValues.FLOW_IN_PORT_NAME].GetConnectionValue());
    }
}
