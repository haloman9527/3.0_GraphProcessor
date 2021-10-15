
namespace CZToolKit.GraphProcessor
{
    public partial class BasePort
    {
        public enum Direction { Input, Output }
        public enum Orientation { Horizontal, Vertical }
        public enum Capacity { Single, Multi }

        public readonly string name;
        public readonly Orientation orientation;
        public readonly Direction direction;
        public readonly Capacity capacity;

        public BasePort(string name, Orientation orientation, Direction direction, Capacity capacity)
        {
            this.name = name;
            this.orientation = orientation;
            this.direction = direction;
            this.capacity = capacity;
        }
    }
}
