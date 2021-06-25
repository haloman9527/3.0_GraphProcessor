
namespace CZToolKit.GraphProcessor
{
    [NodeMenuItem("Math", "Add")]
    public class AddNode : BaseNode
    {
        [Port(PortDirection.Input, IsMulti = true)]
        public float input;

        [Port(PortDirection.Output, IsMulti = true)]
        public float output;

        public override object GetValue(NodePort _port)
        {
            switch (_port.FieldName)
            {
                case nameof(output):
                    float inputSum = 0;
                    foreach (var value in GetConnectValues(nameof(input)))
                    {
                        inputSum += (float)value;
                    }
                    return inputSum;
            }

            return false;
        }
    }
}
