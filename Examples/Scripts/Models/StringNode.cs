using UnityEngine;

namespace CZToolKit.GraphProcessor
{
    [NodeMenuItem("String")]
    public class StringNode : BaseNode
    {
        #region Model
        [Output]
        [SerializeField] string value = "";
        #endregion

        #region ViewModel
        public string Value
        {
            get { return GetPropertyValue<string>(nameof(Value)); }
            set { SetPropertyValue(nameof(Value), value); }
        }

        public override void InitializeBindableProperties()
        {
            base.InitializeBindableProperties();
            SetBindableProperty(nameof(Value), new BindableProperty<string>(value, v => value = v));
        }

        public override object GetValue(NodePort _localPort)
        {
            return Value;
        }
        #endregion
    }
}
