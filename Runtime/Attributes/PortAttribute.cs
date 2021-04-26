using System;

namespace CZToolKit.GraphProcessor
{
    /// <summary> 接口特性，标记此特性的字段将被绘制为一个接口 </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class PortAttribute : Attribute
    {
        /// <summary> 接口方向 </summary>
        public readonly PortDirection Direction;

        /// <summary> 接口数量规则 </summary>
        public bool IsMulti = true;

        /// <summary> 接口类型匹配规则 </summary>
        public PortTypeConstraint TypeConstraint = PortTypeConstraint.Inherited;

        /// <summary> 是否绘制字段 </summary>
        public ShowBackingValue ShowBackValue = ShowBackingValue.Never;

        public PortAttribute(PortDirection _direction, bool _isMulti = true, PortTypeConstraint _typeConstraint = PortTypeConstraint.Inherited)
        {
            Direction = _direction;
            IsMulti = _isMulti;
            TypeConstraint = _typeConstraint;
        }
    }
}