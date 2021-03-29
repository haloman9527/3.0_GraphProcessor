using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace GraphProcessor
{
    [Serializable]
    [NodeMenuItem("Utils", "Relay")]
    public class RelayNode : BaseNode
    {
        [HideInInspector]
        public struct PackedRelayData
        {
            public List<object> values;
            public List<string> names;
            public List<Type> types;
        }

        [Port(PortDirection.Input, IsMulti = false, DisplayName = "In", TypeConstraint = PortTypeConstraint.None)]
        [PortSize(12)]
        public PackedRelayData input;

        [Port(PortDirection.Output, IsMulti = false, DisplayName = "Out", TypeConstraint = PortTypeConstraint.None)]
        [PortSize(12)]
        public PackedRelayData output;

        public override bool GetValue<T>(NodePort _port, ref T _value)
        {
            switch (_port.FieldName)
            {
                case nameof(input):
                    return TryGetOutputValue<T>(nameof(output), out _value);
                case nameof(output):
                    return TryGetOutputValue<T>(nameof(input), out _value);
                default:
                    return false;
            }
        }

        public override void Execute(NodePort _port, params object[] _params)
        {
            switch (_port.Direction)
            {
                case PortDirection.Input:
                    if (TryGetPort("output", out NodePort outputPort) && outputPort.IsConnected)
                        outputPort.Connection.Execute(_params);
                    break;
                case PortDirection.Output:
                    if (TryGetPort("input", out NodePort intputPort) && intputPort.IsConnected)
                        intputPort.Connection.Execute(_params);
                    break;
                default:
                    break;
            }
        }
    }
}