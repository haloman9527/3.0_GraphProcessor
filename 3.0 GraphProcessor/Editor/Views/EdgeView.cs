using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEngine;

namespace GraphProcessor.Editors
{
    public class EdgeView : Edge
    {
        const string EdgeStylePath = "GraphProcessorStyles/EdgeView";
        static StyleSheet edgeViewStyle;
        public static StyleSheet EdgeViewStyle
        {
            get
            {
                if (edgeViewStyle == null)
                    edgeViewStyle = Resources.Load<StyleSheet>(EdgeStylePath);
                return edgeViewStyle;
            }
        }

        public bool isConnected = false;

        public SerializableEdge serializedEdge { get { return userData as SerializableEdge; } }

        protected BaseGraphView owner => ((input ?? output) as PortView).Owner.Owner;

        public EdgeView() : base()
        {
            styleSheets.Add(EdgeViewStyle);
            RegisterCallback<MouseDownEvent>(OnMouseDown);
        }

        public override void OnPortChanged(bool isInput)
        {
            base.OnPortChanged(isInput);
            UpdateEdgeSize();
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
            //*****
            // 双击创建一个RelayNode
            if (e.clickCount == 2)
            {
                // Empirical offset:
                var position = e.mousePosition;
                position += new Vector2(-10f, -28);
                Vector2 mousePos = owner.ChangeCoordinatesTo(owner.contentViewContainer, position);
                owner.Disconnect(this);
                owner.AddRelayNode(input as PortView, output as PortView, mousePos);
            }
        }
    }
}