using UnityEngine;
using System;

namespace CZToolKit.GraphProcessor
{
    [Serializable]
    [NodeMenuItem("Utils", "Relay")]
    public class RelayNode : BaseNode
    {
        [Port(PortDirection.Input, IsMulti = false, TypeConstraint = PortTypeConstraint.None)]
        [InspectorName("In")]
        [PortSize(12), PortColor(0, 0.7f, 0.3f)]
        object input;

        [Port(PortDirection.Output, IsMulti = false, TypeConstraint = PortTypeConstraint.None)]
        [InspectorName("Out")]
        [PortSize(12), PortColor(0, 0.7f, 0.3f)]
        object output;

        public override object GetValue(NodePort _port)
        {
            switch (_port.FieldName)
            {
                case nameof(input):
                    return GetConnectValue(nameof(output));
                case nameof(output):
                    return GetConnectValue(nameof(input));
                default:
                    return null;
            }
        }

        public override void Execute(NodePort _port, params object[] _params)
        {
            switch (_port.Direction)
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