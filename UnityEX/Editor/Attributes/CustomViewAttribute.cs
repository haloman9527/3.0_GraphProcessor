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
#if UNITY_EDITOR
using System;

namespace Moyo.GraphProcessor.Editors
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
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