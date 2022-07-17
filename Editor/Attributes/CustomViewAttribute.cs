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
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class CustomViewAttribute : Attribute
    {
        public Type viewModelType;

        public CustomViewAttribute(Type viewModelType)
        {
            this.viewModelType = viewModelType;
        }
    }
}
#endif