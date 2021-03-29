using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GraphProcessor
{
    [NodeMenuItem("Math", "Float")]
    public class FloatNode : BaseNode
    {
        [Port(PortDirection.Output)]
        public float value;

        public override bool GetValue<T>(NodePort _port, ref T _value)
        {
            switch (_port.FieldName)
            {
                case "value":
                    if (value is T tValue)
                    {
                        _value = tValue;
                        return true;
                    }
                    break;
            }

            return false;
        }
    }
}
