using System;

namespace CZToolKit.GraphProcessor
{
    [Serializable]
    [NodeMenuItem("Math", "Float")]
    public class FloatNode : BaseNode
    {
        [Port(PortDirection.Output)]
        public float value;

        public override object GetValue(NodePort _port)
        {
            return value;
        }
    }
}
