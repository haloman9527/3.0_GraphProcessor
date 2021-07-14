using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEngine;

namespace CZToolKit.GraphProcessor.Editors
{
    public class BaseEdgeView : Edge, IBindableView<BaseEdge>
    {
        public BaseEdge Model { get; private set; }
        protected BaseGraphView Owner { get; private set; }
        protected CommandDispatcher CommandDispatcher { get; private set; }

        public BaseEdgeView() : base()
        {
            styleSheets.Add(GraphProcessorStyles.EdgeViewStyle);

            // 添加流程点
            this.AddManipulator(new FlowPoint());
        }

        public void SetUp(BaseEdge _edge, CommandDispatcher _commandDispatcher, BaseGraphView _graphView)
        {
            Model = _edge;
            CommandDispatcher = _commandDispatcher;
            Owner = _graphView;

            RegisterCallback<MouseDownEvent>(OnMouseDown);
        }

        public virtual void UnBindingProperties()
        {

        }

        void OnMouseDown(MouseDownEvent e)
        {
            // 双击创建一个RelayNode
            if (e.clickCount == 2)
            {
                var position = e.mousePosition;
                position += new Vector2(-20 * Owner.scale, -30 * Owner.scale);
                Vector2 mousePos = Owner.GraphWindow.rootVisualElement.ChangeCoordinatesTo(Owner.contentViewContainer, position);

                Owner.Model.AddRelayNode(Owner.Model.Edges[Model.GUID], mousePos);
            }
        }
    }

    public class FlowPoint : Manipulator
    {
        VisualElement point { get; set; }

        protected override void RegisterCallbacksOnTarget()
        {
            if (target is Edge edge)
            {
                point = new VisualElement();
                point.AddToClassList("flow-point");
                point.style.left = 0;
                point.style.top = 0;
                target.Add(point);

                target.schedule.Execute(() =>
                {
                    UpdateCapPoint(edge, (float)(EditorApplication.timeSinceStartup % 3 / 3));
                }).Until(() => point == null);
            }
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            if (point != null)
            {
                target.Remove(point);
                point = null;
            }
        }

        public void UpdateCapPoint(Edge _edge, float _t)
        {
            Vector3 v = Lerp(_edge.edgeControl.controlPoints, _t);
            point.style.left = v.x;
            point.style.top = v.y;
        }

        Vector2 Lerp(Vector2[] points, float t)
        {
            t = Mathf.Clamp01(t);
            float totalLength = 0;
            for (int i = 0; i < points.Length - 1; i++)
            {
                totalLength += Vector2.Distance(points[i], points[i + 1]);
            }

            float pointLength = Mathf.Lerp(0, totalLength, t);

            float tempLength = 0;
            for (int i = 0; i < points.Length - 1; i++)
            {
                float d = Vector2.Distance(points[i], points[i + 1]);
                if (pointLength <= tempLength + d)
                    return Vector2.Lerp(points[i], points[i + 1], (pointLength - tempLength) / d);
                tempLength += d;
            }
            return points[0];
        }
    }
}