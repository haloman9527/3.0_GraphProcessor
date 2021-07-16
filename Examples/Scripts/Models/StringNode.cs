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
