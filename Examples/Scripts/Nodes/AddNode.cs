
namespace CZToolKit.GraphProcessor
{
    [NodeMenuItem("Math", "Add")]
    public class AddNode : BaseNode
    {
        [Port(PortDirection.Input, IsMulti = true)]
        [ShowAsDrawer]
        float input;

        [Port(PortDirection.Output, IsMulti = true)]
        float output;

        public override bool GetValue<T>(NodePort _port, ref T _value)
        {
            switch (_port.FieldName)
            {
                case nameof(output):
                    float inputSum = 0;
                    foreach (var value in GetConnectValues<float>(nameof(input)))
                    {
                        inputSum += value;
                    }
                    if (inputSum is T tValue)
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
