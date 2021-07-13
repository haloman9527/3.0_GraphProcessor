using System;
using UnityEngine;

namespace CZToolKit.GraphProcessor
{
    [Serializable]
    [NodeMenuItem("Math", "Float")]
    public class FloatNode : BaseNode
    {
        #region Model
        [Port(PortDirection.Output)]
        [SerializeField] float value;
        #endregion

        #region ViewModel
        public float Value
        {
            get { return GetPropertyValue<float>(nameof(Value)); }
            set { SetPropertyValue(nameof(FloatNode.value), value); }
        }

        public override void InitializeBindableProperties()
        {
            base.InitializeBindableProperties();
            SetBindableProperty(nameof(Value), new BindableProperty<float>(value, v => value = v));
        }

        public override object GetValue(NodePort _localPort)
        {
            return Value;
        }
        #endregion
    }
}
