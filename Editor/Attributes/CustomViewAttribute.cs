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
#if UNITY_EDITOR
using System;

namespace CZToolKit.GraphProcessor.Editors
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class CustomViewAttribute : Attribute
    {
        public Type targetType;

        public CustomViewAttribute(Type targetType)
        {
            this.targetType = targetType;
        }
    }
}
#endif