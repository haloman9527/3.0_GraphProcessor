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

namespace CZToolKit.GraphProcessor
{

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class NodeDescriptionAttribute : Attribute
    {
        public string description = string.Empty;
        public NodeDescriptionAttribute(string _description)
        {
            description = _description;
        }
    }
}
