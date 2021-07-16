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
using CZToolKit.Core.SharedVariable;
using UnityEngine;

namespace CZToolKit.GraphProcessor.Examples
{
    [NodeMenuItem("Examples", "SharedVariable")]
    public class SharedVariableNode : BaseNode
    {
        #region Model
        [Output]
        [SerializeField] SharedGameObject value;
        #endregion

        #region ViewModel
        public GameObject Value
        {
            get { return GetPropertyValue<GameObject>(nameof(Value)); }
            set { SetPropertyValue(nameof(Value), value); }
        }

        public override void InitializeBindableProperties()
        {
            base.InitializeBindableProperties();
            SetBindableProperty(nameof(Value), new BindableProperty<GameObject>(value.Value, v => value.Value = v));
        }

        public override object GetValue(NodePort _localPort)
        {
            return Value;
        }
        #endregion
    }
}
