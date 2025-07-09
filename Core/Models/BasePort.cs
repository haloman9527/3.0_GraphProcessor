#region 注 释

/***
 *
 *  Title:
 *
 *  Description:
 *
 *  Date:
 *  Version:
 *  Writer: 半只龙虾人
 *  Github: https://github.com/haloman9527
 *  Blog: https://www.haloman.net/
 *
 */

#endregion

using System;
using System.Collections.Generic;
using System.Linq;

namespace Atom.GraphProcessor
{
    public class BasePort
    {
        #region Define

        public enum Direction
        {
            Left,
            Right,
            Top,
            Bottom,
        }

        public enum Capacity
        {
            Single,
            Multi
        }

        #endregion

        public string name;
        public Direction direction;
        public Capacity capacity;
        public Type portType;

        public BasePort(string name, BasePort.Direction direction, BasePort.Capacity capacity, Type type = null)
        {
            this.name = name;
            this.direction = direction;
            this.capacity = capacity;
            this.portType = type;
        }
    }
}