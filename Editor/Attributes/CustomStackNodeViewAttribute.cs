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
    [Obsolete]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class CustomStackNodeView : Attribute
    {
        public Type stackType;

        public CustomStackNodeView(Type _stackNodeType)
        {
            this.stackType = _stackNodeType;
        }
    }
}