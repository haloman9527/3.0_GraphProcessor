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
    [Serializable]
    [NodeMenuItem("Utils", "Relay")]
    public class RelayNode : BaseNode
    {
        #region Model
        [Port(PortDirection.Input, IsMulti = false, TypeConstraint = PortTypeConstraint.None)]
        [PortColor(0, 0.7f, 0.3f)]
        public object input;

        [Port(PortDirection.Output, IsMulti = false, TypeConstraint = PortTypeConstraint.None)]
        [PortColor(0, 0.7f, 0.3f)]
        public object output;
        #endregion

        #region ViewModel
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
        #endregion
    }
}