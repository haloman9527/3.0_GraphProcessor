
using System;

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
        public readonly Type type;

        public BasePort(string name, Orientation orientation, Direction direction, Capacity capacity, Type type = null)
        {
            this.name = name;
            this.orientation = orientation;
            this.direction = direction;
            this.capacity = capacity;
            this.type = type == null ? typeof(object) : type;
        }
    }
}
