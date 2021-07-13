using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEngine;

namespace CZToolKit.GraphProcessor.Editors
{
    public class BaseEdgeView : Edge
    {
        protected BaseGraphView Owner { get; private set; }
        protected CommandDispatcher CommandDispatcher { get; private set; }
        public BaseEdge Model { get; private set; }

        public BaseEdgeView() : base()
        {
            styleSheets.Add(GraphProcessorStyles.EdgeViewStyle);

            // 添加流程点
            this.AddManipulator(new ProcessPoint());
        }

        public void SetUp(BaseEdge _edge, CommandDispatcher _commandDispatcher, BaseGraphView _graphView)
        {
            CommandDispatcher = _commandDispatcher;
            Owner = _graphView;

            Model = _edge;
            BindingProperties();
            Model.UpdateProperties();

            RegisterCallback<MouseDownEvent>(OnMouseDown);
        }

        protected virtual void BindingProperties()
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

    public class ProcessPoint : Manipulator
    {
        VisualElement capPoint { get; set; }

        protected override void RegisterCallbacksOnTarget()
        {
            if (target is Edge edge)
            {
                capPoint = new VisualElement();
                capPoint.AddToClassList("cap-point");
                capPoint.style.left = 0;
                capPoint.style.top = 0;
                target.Add(capPoint);

                target.schedule.Execute(_ =>
                {
                    UpdateCapPoint(edge, (float)(EditorApplication.timeSinceStartup % 3 / 3));
                }).Until(() => capPoint == null);
            }
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            if (capPoint != null)
            {
                target.Remove(capPoint);
                capPoint = null;
            }
        }

        public void UpdateCapPoint(Edge _edge, float _t)
        {
            Vector3 v = Lerp(_edge.edgeControl.controlPoints, _t);
            capPoint.style.left = v.x;
            capPoint.style.top = v.y;
        }

        Vector2 Lerp(Vector2[] points, float t)
        {
            t = Mathf.Clamp01(t);
            float length = 0;
            for (int i = 0; i < points.Length - 1; i++)
            {
                length += Vector2.Distance(points[i], points[i + 1]);
            }

            float p = Mathf.Lerp(0, length, t);

            float n = 0;
            for (int i = 0; i < points.Length - 1; i++)
            {
                float d = Vector2.Distance(points[i], points[i + 1]);
                if (p <= n + d)
                    return Vector2.Lerp(points[i], points[i + 1], (p - n) / d);
                n += d;
            }
            return points[0];
        }
    }
}