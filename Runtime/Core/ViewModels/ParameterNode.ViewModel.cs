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
using CZToolKit.Core.Blackboards;

namespace CZToolKit.GraphProcessor
{
    public partial class ParameterNode : BaseNode
    {
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
                port.Value.DisplayType = Parameter?.ValueType;
            }
        }

        public override void InitializeBindableProperties()
        {
            base.InitializeBindableProperties();
            this[nameof(Name)]= new BindableProperty<string>(name, v => name = v);
        }

        public override object GetValue(NodePort _localPort)
        {
            if (Parameter == null)
                return null;
            return Parameter.GetValue();
        }
    }
}
