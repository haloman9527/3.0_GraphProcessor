using System;
using System.Collections.Generic;

namespace CZToolKit.GraphProcessor
{
    [Serializable]
    [NodeMenuItem("Math", "Float")]
    public class FloatNode : BaseNode
    {
        [Port(PortDirection.Output)]
        public float value;

        public List<int> nums;

        public override bool GetValue<T>(NodePort _port, ref T _value)
        {
            if (value is T tValue)
            {
                _value = tValue;
                return true;
            }
            return false;
        }
    }
}
