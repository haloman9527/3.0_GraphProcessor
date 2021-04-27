using System;
using UnityEngine;

namespace CZToolKit.GraphProcessor
{
    [Serializable]
    [NodeMenuItem("Parameter", ShowInList = false)]
    public class ParameterNode : BaseNode
    {
        [Port(PortDirection.Output)]
        [DisplayName("Value")]
        [PortSize(12)]
        [Tooltip("接口无限制，参数提供")]
        [HideInInspector]
        public object output;

        [Port(PortDirection.Input)]
        public object output1;

        [HideInInspector]
        public string paramGUID;

        public ExposedParameter Parameter
        {
            get { return Owner.TryGetExposedParameterFromGUID(paramGUID, out ExposedParameter param) ? param : null; }
        }

        public override bool GetValue<T>(NodePort _port, ref T _value)
        {
            if (Parameter.Value is T tValue)
            {
                _value = tValue;
                return true;
            }
            return false;
        }

        public override Type PortDynamicType(string _portName)
        {
            if (_portName == nameof(output) && Parameter?.ValueType != null)
                return Parameter.ValueType;
            return null;
        }
    }
}
