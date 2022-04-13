#region 注 释
/***
 *
 *  Title:
 *  
 *  Description:
 *      把节点绑定至Graph
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
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class AttachableAttribute : Attribute
    {
        public readonly Type Type;
        public AttachableAttribute(Type type)
        {
            this.Type = type;
        }
    }
}
