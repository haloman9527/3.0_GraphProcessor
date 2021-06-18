using UnityEngine;

namespace CZToolKit.GraphProcessor
{
    [NodeMenuItem("String")]
    [NodeIcon("Assets/CZToolKit/0.1_GOAP/Editor/Icons/Running.png", width = 15, height = 18)]
    public class StringNode : BaseNode
    {
        [TextArea]
        [Port(PortDirection.Output)]
        public string value = "";

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
