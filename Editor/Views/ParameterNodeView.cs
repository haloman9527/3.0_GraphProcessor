using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace CZToolKit.GraphProcessor.Editors
{
    [CustomNodeView(typeof(ParameterNode))]
    public class ParameterNodeView : SimpleNodeView<ParameterNode>
    {
        void OnDataNameChanged(string _dataName)
        {
            title = _dataName;
        }

        protected override void BindingPropertiesBeforeUpdate()
        {
            base.BindingPropertiesBeforeUpdate();
            T_Model.RegisterValueChangedEvent<string>(nameof(T_Model.Name), OnDataNameChanged);
        }
        public override void UnBindingProperties()
        {
            base.UnBindingProperties();
            T_Model.UnregisterValueChangedEvent<string>(nameof(T_Model.Name), OnDataNameChanged);
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();
            title = T_Model.Name;

            RegisterCallback<MouseEnterEvent>(_ => { OnMouseEnter(); });
            RegisterCallback<MouseLeaveEvent>(_ => { OnMouseLeave(); });
            RegisterCallback<DetachFromPanelEvent>(_ => { OnMouseLeave(); });

            foreach (var portView in PortViews.Values)
            {
                portView.tooltip = T_Model.Parameter?.ValueType.ToString();
            }
        }

        protected override NodePortView CustomCreatePortView(Orientation _orientation, Direction _direction, NodePort _nodePort)
        {
            if (T_Model.Parameter == null || T_Model.Parameter.ValueType == null)
                return null;
            if (_nodePort.FieldName == "output")
                return NodePortView.CreatePV(_orientation, _direction, _nodePort, T_Model.Parameter.ValueType);
            return null;
        }

        void OnMouseEnter()
        {
            (Owner.Blackboard.Fields[T_Model.Name].Q(className: "blackboardField") as BlackboardField).highlighted = true;
        }

        void OnMouseLeave()
        {
            (Owner.Blackboard.Fields[T_Model.Name].Q(className: "blackboardField") as BlackboardField).highlighted = false;
        }
    }
}
