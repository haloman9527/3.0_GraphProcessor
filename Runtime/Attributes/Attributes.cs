using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace GraphProcessor
{

    /// <summary> Set a custom drawer for a field. It can then be created using the FieldFactory </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class FieldDrawerAttribute : Attribute
    {
        public Type fieldType;

        /// <summary> Register a custom view for a type in the FieldFactory class </summary>
        public FieldDrawerAttribute(Type fieldType)
        {
            this.fieldType = fieldType;
        }
    }

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
        /// <param name="fieldName">local field of the node</param>
        /// <param name="inputType">type of input of the port</param>
        /// <param name="allowCast">if cast is allowed when connecting an edge</param>
        public CustomPortInputAttribute(string fieldName, Type inputType, bool allowCast = true)
        {
            this.fieldName = fieldName;
            this.inputType = inputType;
            this.allowCast = allowCast;
        }
    }

    /// <summary> Allow you to customize the input function of a port </summary>
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
        /// <param name="fieldName">local field of the node</param>
        /// <param name="inputType">type of input of the port</param>
        /// <param name="allowCast">if cast is allowed when connecting an edge</param>
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

        /// <summary> Allow you to have a custom view for your stack nodes </summary>
        /// <param name="stackNodeType">The type of the stack node you target</param>
        public CustomStackNodeView(Type stackNodeType)
        {
            this.stackNodeType = stackNodeType;
        }
    }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class ShowAsDrawer : Attribute
    {
        public ShowAsDrawer() { }
    }
}