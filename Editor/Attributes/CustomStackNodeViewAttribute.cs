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
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class CustomStackNodeView : Attribute
    {
        public Type stackNodeType;

        public CustomStackNodeView(Type _stackNodeType)
        {
            this.stackNodeType = _stackNodeType;
        }
    }
}