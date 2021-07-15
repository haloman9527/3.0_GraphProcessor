using CZToolKit.Core.Blackboards;
using System;
using UnityEngine;

namespace CZToolKit.GraphProcessor
{
    [Serializable]
    [NodeMenuItem("Parameter", showInList = false)]
    public class ParameterNode : BaseNode
    {
        #region Model
        [Port(PortDirection.Output)]
        [InspectorName("Value")]
        [SerializeField] object output;

        [HideInInspector]
        [SerializeField] string name;
        #endregion

        #region ViewModel
        public string Name
        {
            get { return GetPropertyValue<string>(nameof(Name)); }
            set
            {
                SetPropertyValue(nameof(Name), value);
            }
        }

        public ICZType Parameter
        {
            get { return Owner.Blackboard.TryGetData(Name, out ICZType param) ? param : null; }
        }

        public override void Enable(BaseGraph _graph)
        {
            base.Enable(_graph);
            foreach (var port in Ports)
            {
                port.Value.DisplayType = Parameter.ValueType;
            }
        }

        public override void InitializeBindableProperties()
        {
            base.InitializeBindableProperties();
            SetBindableProperty(nameof(Name), new BindableProperty<string>(name, v => name = v));
        }

        public override object GetValue(NodePort _localPort)
        {
            if (Parameter == null)
                return null;
            return Parameter.GetValue();
        }
        #endregion
    }
}
