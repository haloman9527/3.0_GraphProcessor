#region 注 释
/***
 *
 *  Title:
 *  
 *  Description:
 *  
 *  Date:
 *  Version:
 *  Writer: 
 *
 */
#endregion
using System;

namespace CZToolKit.GraphProcessor.Editors
{
    public class CustomParameterNodeViewAttribute : Attribute
    {
        public Type targetType;

        /// <summary>  </summary>
        /// <param name="_targetType">参数的类型</param>
        public CustomParameterNodeViewAttribute(Type _targetType)
        {
            targetType = _targetType;
        }
    }
}
