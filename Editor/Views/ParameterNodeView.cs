using UnityEditor.Experimental.GraphView;

namespace CZToolKit.GraphProcessor.Editors
{
    [CustomNodeView(typeof(ParameterNode))]
    public class ParameterNodeView : SimpleNodeView
    {
        protected override void OnInitialized()
        {
            base.OnInitialized();

            ParameterNode parameterNode = NodeData as ParameterNode;
            title = parameterNode.name;

            //PortViews[nameof(parameterNode.output)].tooltip = parameterNode.Parameter.ValueType.Name;

            //Add(new IMGUIContainer(() =>
            //{
            //    tooltip = parameterNode.Parameter?.Value.ToString();
            //}));
        }

        protected override PortView CustomCreatePortView(Orientation _orientation, Direction _direction, NodePort _nodePort, BaseEdgeConnectorListener _listener)
        {
            ParameterNode parameterNode = NodeData as ParameterNode;
            if (parameterNode.Parameter == null || parameterNode.Parameter.ValueType == null)
                return null;
            if (_nodePort.FieldName == nameof(parameterNode.output))
                return PortView.CreatePV(_orientation, _direction, _nodePort, parameterNode.Parameter.ValueType, _listener);
            return null;
        }
    }
}
