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
 *  Github: https://github.com/HalfLobsterMan
 *  Blog: https://www.crosshair.top/
 *
 */
#endregion
using System;
using UnityEngine;

namespace CZToolKit.GraphProcessor
{
    [Serializable]
    public partial class BasePort
    {
        public enum Direction { Input, Output }
        public enum Orientation { Horizontal, Vertical }
        public enum Capacity { Single, Multi }

        [SerializeField] internal readonly string name;
        [SerializeField] internal readonly Orientation orientation;
        [SerializeField] internal readonly Direction direction;
        [SerializeField] internal readonly Capacity capacity;
        [SerializeField] internal Type type;

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
