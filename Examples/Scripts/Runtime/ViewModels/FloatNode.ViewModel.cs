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
using UnityEngine;

namespace CZToolKit.GraphProcessor
{
    public partial class FloatNode : BaseNode
    {
        public float Value
        {
            get { return GetPropertyValue<float>(nameof(Value)); }
            set { SetPropertyValue(nameof(FloatNode.value), value); }
        }

        public override void InitializeBindableProperties()
        {
            base.InitializeBindableProperties();
            this[nameof(Value)] = new BindableProperty<float>(value, v => value = v);
        }

        public override object GetValue(NodePort _localPort)
        {
            return Value;
        }
    }
}
