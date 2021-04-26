using System;

namespace CZToolKit.GraphProcessor
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Class, AllowMultiple = false)]
    public class DisplayNameAttribute : Attribute
    {
        /// <summary> 自定义显示名称 </summary>
        public string DisplayName;

        public DisplayNameAttribute(string _displayName)
        {
            DisplayName = _displayName;
        }
    }
}
