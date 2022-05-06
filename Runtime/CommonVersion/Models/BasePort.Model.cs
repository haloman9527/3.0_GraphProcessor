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

        [SerializeField] [HideInInspector] internal readonly string name;
        [SerializeField] [HideInInspector] internal readonly Orientation orientation;
        [SerializeField] [HideInInspector] internal readonly Direction direction;
        [SerializeField] [HideInInspector] internal readonly Capacity capacity;
        [SerializeField] [HideInInspector] internal Type type;

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
