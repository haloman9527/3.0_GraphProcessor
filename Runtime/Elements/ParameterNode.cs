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

        public ExposedParameter Parameter
        {
            get { return Owner.GetExposedParameterFromGUID(paramGUID); }
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

        //public override void Execute(NodePort _port, params object[] _params)
        //{
        //    if (Parameter != null && _params != null && _params.Length != 0)
        //        Parameter.Value = _params[0];
        //}
    }
}
