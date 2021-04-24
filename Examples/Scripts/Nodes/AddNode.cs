
namespace GraphProcessor
{
    [NodeMenuItem("Math", "Add")]
    public class AddNode : BaseNode
    {
        [Port(PortDirection.Input, IsMulti = false)]
        public float a, b;

        [Port(PortDirection.Output, IsMulti = false)]
        float output;

        public override bool GetValue<T>(NodePort _port, ref T _value)
        {
            TryGetInputValue(nameof(a), out float tempA, a);
            TryGetInputValue(nameof(b), out float tempB, b);

            if ((tempA + tempB) is T tValue) { _value = tValue; return true; }
            else return false;
        }
    }
}
