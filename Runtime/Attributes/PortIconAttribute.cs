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
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class PortIconAttribute : Attribute
    {
        public string iconPath;

        public PortIconAttribute(string _iconPath)
        {
            iconPath = _iconPath;
        }
    }
}
