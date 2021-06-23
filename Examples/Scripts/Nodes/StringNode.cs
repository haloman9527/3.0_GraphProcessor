using UnityEngine;

namespace CZToolKit.GraphProcessor
{
    [NodeMenuItem("String")]
    public class StringNode : BaseNode
    {
        [TextArea]
        [Port(PortDirection.Output)]
        public string value = "";

        public override object GetValue(NodePort _localPort)
        {
            return value;
        }
    }
}
