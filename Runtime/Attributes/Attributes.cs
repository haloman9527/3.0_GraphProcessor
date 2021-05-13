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
    /// <summary> Allow you to customize the input function of a port </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class CustomPortInputAttribute : Attribute
    {
        public string fieldName;
        public Type inputType;
        public bool allowCast;

        /// <summary>
        /// Allow you to customize the input function of a port.
        /// See CustomPortsNode example in Samples.
        /// </summary>
        public CustomPortInputAttribute(string fieldName, Type inputType, bool allowCast = true)
        {
            this.fieldName = fieldName;
            this.inputType = inputType;
            this.allowCast = allowCast;
        }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class CustomPortOutputAttribute : Attribute
    {
        public string fieldName;
        public Type outputType;
        public bool allowCast;

        /// <summary>
        /// Allow you to customize the output function of a port.
        /// See CustomPortsNode example in Samples.
        /// </summary>
        public CustomPortOutputAttribute(string fieldName, Type outputType, bool allowCast = true)
        {
            this.fieldName = fieldName;
            this.outputType = outputType;
            this.allowCast = allowCast;
        }
    }

    /// <summary> Allow you to have a custom view for your stack nodes </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class CustomStackNodeView : Attribute
    {
        public Type stackNodeType;

        public CustomStackNodeView(Type stackNodeType)
        {
            this.stackNodeType = stackNodeType;
        }
    }
}