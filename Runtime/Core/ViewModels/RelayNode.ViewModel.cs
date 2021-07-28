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
using System;

namespace CZToolKit.GraphProcessor
{
    public partial class RelayNode : BaseNode
    {
        public override object GetValue(NodePort _localPort)
        {
            switch (_localPort.FieldName)
            {
                case nameof(RelayNode.input):
                    return GetConnectValue(nameof(RelayNode.output));
                case nameof(RelayNode.output):
                    return GetConnectValue(nameof(RelayNode.input));
                default:
                    return null;
            }
        }

        public override void Execute(NodePort _localPort, params object[] _params)
        {
            switch (_localPort.Direction)
            {
                case PortDirection.Input:
                    if (TryGetPort(nameof(input), out NodePort outputPort) && outputPort.IsConnected)
                        outputPort.Connection.Execute(_params);
                    break;
                case PortDirection.Output:
                    if (TryGetPort(nameof(output), out NodePort intputPort) && intputPort.IsConnected)
                        intputPort.Connection.Execute(_params);
                    break;
                default:
                    break;
            }
        }
    }
}