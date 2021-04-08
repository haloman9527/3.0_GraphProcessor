using CZToolKit.Core.Blackboards;
using System;
using UnityEngine;

namespace GraphProcessor
{
    [Serializable]
    [NodeMenuItem("Parameter", ShowInList = false)]
    public class ParameterNode : BaseNode
    {
        [Port(PortDirection.Output, DisplayName = "Value")]
        [PortSize(12)]
        [Tooltip("接口无限制，参数提供")]
        [HideInInspector]
        public object output;

        [HideInInspector]
        public string paramGUID;

        public IBlackboardPropertyGUID Parameter
        {
            get { return Owner.Blackboard.TryGetParamFromGUID(paramGUID, out IBlackboardProperty param) ? param as IBlackboardPropertyGUID : null; }
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

        public override Type PortDynamicType(NodePort _port)
        {
            if (_port.FieldName == nameof(output) && Parameter?.PropertyType != null)
                return Parameter.PropertyType;
            return null;
        }
    }
}
