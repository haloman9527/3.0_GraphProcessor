using CZToolKit.Core.Blackboards;
using System;
using UnityEngine;

namespace CZToolKit.GraphProcessor
{
    [Serializable]
    [NodeMenuItem("Parameter", ShowInList = false)]
    public class ParameterNode : BaseNode
    {
        [Port(PortDirection.Output)]
        [InspectorName("Value")]
        [PortSize(12)]
        [Tooltip("接口无限制，参数提供")]
        [HideInInspector]
        public object output;

        [HideInInspector]
        public string name;

        public ICZType Parameter
        {
            get { return Owner.Blackboard.TryGetData(name, out ICZType param) ? param : null; }
        }

        public override object GetValue(NodePort _port)
        {
            return Parameter.GetValue();
        }

        public override Type PortDynamicType(string _portName)
        {
            if (_portName == nameof(output) && Parameter != null)
                return Parameter.ValueType;
            return null;
        }
    }
}
