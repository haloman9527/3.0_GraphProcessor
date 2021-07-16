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

namespace CZToolKit.GraphProcessor
{
    /// <summary> 接口特性，标记此特性的字段将被绘制为一个接口 </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class PortAttribute : Attribute
    {
        /// <summary> 接口方向 </summary>
        public readonly PortDirection Direction;

        /// <summary> 接口数量规则 </summary>
        public bool IsMulti = true;

        /// <summary> 接口类型匹配规则 </summary>
        public PortTypeConstraint TypeConstraint = PortTypeConstraint.Inherited;

        public PortAttribute(PortDirection _direction, bool _isMulti = true, PortTypeConstraint _typeConstraint = PortTypeConstraint.Inherited)
        {
            Direction = _direction;
            IsMulti = _isMulti;
            TypeConstraint = _typeConstraint;
        }
    }

    public class InputAttribute : PortAttribute
    {
        public InputAttribute(bool _isMulti = true, PortTypeConstraint _typeConstraint = PortTypeConstraint.Inherited) : base(PortDirection.Input, _isMulti, _typeConstraint) { }
    }

    public class OutputAttribute : PortAttribute
    {
        public OutputAttribute(bool _isMulti = true, PortTypeConstraint _typeConstraint = PortTypeConstraint.Inherited) : base(PortDirection.Output, _isMulti, _typeConstraint) { }
    }
}