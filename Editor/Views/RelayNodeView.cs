using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace CZToolKit.GraphProcessor.Editors
{
    [CustomNodeView(typeof(RelayNode))]
    public class RelayNodeView : BaseNodeView
    {
        protected override void OnInitialized()
        {
            titleContainer.RemoveFromHierarchy();
            this.Q("divider").RemoveFromHierarchy();

            styleSheets.Add(Resources.Load<StyleSheet>("GraphProcessor/Styles/RelayNode"));
            foreach (var item in PortViews)
            {
                item.Value.Q("type").style.display = DisplayStyle.None;
            }

            RegisterCallback<MouseDownEvent>(OnMouseDown);
        }

        private void OnMouseDown(MouseDownEvent evt)
        {
            // 双击删除
            if (evt.clickCount == 2)
                Owner.RemoveRelayNode(this);
        }

        public override void OnPortConnected(PortView _portView, PortView _targetPortView)
        {
            base.OnPortConnected(_portView, _targetPortView);
            //if (_portView == PortViews["input"])
            //{
            //    _portView.portType = _targetPortView.portType;
            //    PortViews["output"].portType = _targetPortView.portType;

            //    _portView.portColor = _targetPortView.portColor;
            //    PortViews["output"].portColor = _targetPortView.portColor;

            //    _portView.schedule.Execute(() =>
            //    {
            //        foreach (var edge in _portView.Edges)
            //        {
            //            edge.UpdateEdgeControl();
            //            edge.MarkDirtyRepaint();
            //        }
            //    }).ExecuteLater(50);

            //    PortViews["output"].schedule.Execute(() =>
            //    {
            //        foreach (var edge in PortViews["output"].Edges)
            //        {
            //            edge.UpdateEdgeControl();
            //            edge.MarkDirtyRepaint();
            //        }
            //    }).ExecuteLater(50);
            //}
        }

        public override void OnPortDisconnected(PortView _portView, PortView _targetPortView)
        {
            base.OnPortDisconnected(_portView, _targetPortView);

            //if (_portView == PortViews["input"])
            //{
            //    _portView.portType = NodeData.Ports["input"].DisplayType;
            //    PortViews["output"].portType = NodeData.Ports["output"].DisplayType;

            //    _portView.portColor = Color.white;
            //    PortViews["output"].portColor = Color.white;

            //    _portView.schedule.Execute(() =>
            //    {
            //        foreach (var edge in _portView.Edges)
            //        {
            //            edge.UpdateEdgeControl();
            //            edge.MarkDirtyRepaint();
            //        }
            //    }).ExecuteLater(50);

            //    PortViews["output"].schedule.Execute(() =>
            //    {
            //        foreach (var edge in PortViews["output"].Edges)
            //        {
            //            edge.UpdateEdgeControl();
            //            edge.MarkDirtyRepaint();
            //        }
            //    }).ExecuteLater(50);
            //}
        }
    }
}