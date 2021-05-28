using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEngine;

namespace CZToolKit.GraphProcessor.Editors
{
    public class EdgeView : Edge
    {
        const string EdgeStylePath = "GraphProcessor/Styles/EdgeView";
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
        protected BaseGraphView Owner { get { return ((input ?? output) as PortView).Owner.Owner; } }
        public SerializableEdge EdgeData { get { return userData as SerializableEdge; } }

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