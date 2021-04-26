using UnityEngine;

namespace CZToolKit.GraphProcessor
{
    [NodeMenuItem("String")]
    [NodeIcon("")]
    public class StringNode : BaseNode
    {
        [TextArea]
        [Port(PortDirection.Output)]
        public string value;

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
