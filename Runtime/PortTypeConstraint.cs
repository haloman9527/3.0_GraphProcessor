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

namespace CZToolKit.GraphProcessor
{
    /// <summary> 接口连接类型限制 </summary>
    public enum PortTypeConstraint
    {
        /// <summary> 允许所有类型的连接 </summary>
        None,

        /// <summary> 同类型和子类可连接 </summary>
        Inherited,

        /// <summary> 仅同类型可连接 </summary>
        Strict,
    }
}