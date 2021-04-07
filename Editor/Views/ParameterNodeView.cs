using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace GraphProcessor.Editors
{
    [CustomNodeView(typeof(ParameterNode))]
    public class ParameterNodeView : BaseNodeView
    {
        protected override PortView CustomCreatePortView(Orientation _orientation, Direction _direction, NodePort _nodePort, BaseEdgeConnectorListener _listener)
        {
            ParameterNode parameterNode = NodeData as ParameterNode;
            if (parameterNode.Parameter == null || parameterNode.Parameter.ValueType == null)
                return null;
            if (_nodePort.FieldName == nameof(parameterNode.output))
                return PortView.CreatePV(_orientation, _direction, _nodePort, parameterNode.Parameter.ValueType, _listener);
            return null;
        }

        protected override void OnInitialized()
        {
            ParameterNode parameterNode = NodeData as ParameterNode;

            titleContainer.Remove(titleContainer.Q("title-button-container"));
            topContainer.parent.Remove(topContainer);
            titleContainer.Add(topContainer);

            title = parameterNode.Parameter?.Name;

            PortViews[nameof(parameterNode.output)].portType = parameterNode.Parameter?.ValueType;
        }
    }
}
