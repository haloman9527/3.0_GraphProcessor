using System;
using UnityEngine;

namespace CZToolKit.GraphProcessor
{
    [Serializable]
    [NodeMenuItem("Util", "Debug.Log")]
    public class DebugNode : BaseNode
    {
        #region Model
        [Input(IsMulti = false, TypeConstraint = PortTypeConstraint.None)]
        [TextArea]
        [SerializeField] string input;
        #endregion

        #region ViewModel
        public string Input
        {
            get { return GetPropertyValue<string>(nameof(Input)); }
            set { SetPropertyValue(nameof(Input), value); }
        }

        public override void InitializeBindableProperties()
        {
            base.InitializeBindableProperties();
            SetBindableProperty(nameof(Input), new BindableProperty<string>(input, v => input = v));
        }
        #endregion
    }
}