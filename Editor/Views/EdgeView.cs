using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEngine;

namespace CZToolKit.GraphProcessor.Editors
{
    public class EdgeView : Edge, IEdgeView
    {
        public new class UxmlFactory : UxmlFactory<EdgeView, GraphView.UxmlTraits> { }


        public bool isConnected = false;
        protected BaseGraphView Owner { get; private set; }
        protected CommandDispatcher CommandDispatcher { get; private set; }
        public SerializableEdge EdgeData { get { return userData as SerializableEdge; } }

        public EdgeView() : base()
        {
            styleSheets.Add(GraphProcessorStyles.EdgeViewStyle);
            RegisterCallback<MouseDownEvent>(OnMouseDown);
        }

        public void SetUp(IGraphElement _graphElement, CommandDispatcher _commandDispatcher, IGraphView _graphView)
        {
            userData = _graphElement;
            CommandDispatcher = _commandDispatcher;
            Owner = _graphView as BaseGraphView;

            //Add(new EdgeBubble());
        }

        public override void OnPortChanged(bool isInput)
        {
            base.OnPortChanged(isInput);
            UpdateEdgeSize();
        }

        protected override void DrawEdge()
        {
            base.DrawEdge();

        }

        public void UpdateEdgeSize()
        {
            if (input == null && output == null)
                return;
            PortView inputPortView = input as PortView;
            PortView outputPortView = output as PortView;

            for (int i = 1; i < 20; i++)
                RemoveFromClassList($"edge_{i}");
            int maxPortSize = Mathf.Max(inputPortView?.size ?? 0, outputPortView?.size ?? 0);
            if (maxPortSize > 0)
                AddToClassList($"edge_{Mathf.Max(1, maxPortSize - 6)}");
        }

        protected override void OnCustomStyleResolved(ICustomStyle styles)
        {
            base.OnCustomStyleResolved(styles);

            UpdateEdgeControl();
        }

        void OnMouseDown(MouseDownEvent e)
        {
            // 双击创建一个RelayNode
            if (e.clickCount == 2)
            {
                var position = e.mousePosition;
                position += new Vector2(-10f, -28);
                Vector2 mousePos = Owner.ChangeCoordinatesTo(Owner.contentViewContainer, position);
                Owner.Disconnect(this);
                Owner.AddRelayNode(input as PortView, output as PortView, mousePos);
            }
        }
    }
}