using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace CZToolKit.GraphProcessor.Editors
{
    [CustomNodeView(typeof(ParameterNode))]
    public class ParameterNodeView : SimpleNodeView<ParameterNode>
    {
        protected override void BindingProperties()
        {
            base.BindingProperties();
            T_Model.RegisterValueChangedEvent<string>(nameof(T_Model.Name), v =>
            {
                title = v;
            });
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();
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

        //public override void OnSelected()
        //{
        //    base.OnSelected();
        //    (Owner.Blackboard.Fields[TViewModel.Name].Q(className: "blackboardField") as BlackboardField).highlighted = true;
        //}

        //public override void OnUnselected()
        //{
        //    base.OnUnselected();
        //    (Owner.Blackboard.Fields[TViewModel.Name].Q(className: "blackboardField") as BlackboardField).highlighted = false;
        //}
    }
}
