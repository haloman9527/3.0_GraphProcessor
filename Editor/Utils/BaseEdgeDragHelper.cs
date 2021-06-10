using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;

namespace CZToolKit.GraphProcessor.Editors
{
    public class EdgeViewDragHelper : EdgeDragHelper
    {
        internal const int k_PanAreaWidth = 10;
        internal const int k_PanSpeed = 4;
        internal const int k_PanInterval = 10;
        internal const float k_MinSpeedFactor = 0.5f;
        internal const float k_MaxSpeedFactor = 7f;
        internal const float k_MaxPanSpeed = k_MaxSpeedFactor * k_PanSpeed;
        internal const float kPortDetectionWidth = 30;

        protected List<Port> compatiblePorts = new List<Port>();
        private Edge ghostEdge;
        protected static NodeAdapter nodeAdapter = new NodeAdapter();
        protected readonly IEdgeConnectorListener listener;

        private IVisualElementScheduledItem panSchedule;
        private Vector3 panDiff = Vector3.zero;
        private bool wasPanned;

        protected GraphView GraphView { get; }
        public bool resetPositionOnPan { get; set; }

        public EdgeViewDragHelper(GraphView _graphView, IEdgeConnectorListener listener)
        {
            GraphView = _graphView;
            this.listener = listener;
            resetPositionOnPan = true;
            Reset();
        }

        public override Edge edgeCandidate { get; set; }

        public override Port draggedPort { get; set; }

        public override void Reset(bool _didConnect = false)
        {
            if (compatiblePorts != null && GraphView != null)
            {
                // Reset the highlights.
                GraphView.ports.ForEach((p) =>
                {
                    p.OnStopEdgeDragging();
                });
                compatiblePorts.Clear();
            }

            // Clean up ghost edge.
            if (ghostEdge != null && GraphView != null)
            {
                var pv = ghostEdge.input as PortView;
                pv.portCapLit = false;
                //GraphView.schedule.Execute(() =>
                //{
                //}).ExecuteLater(10);
                GraphView.RemoveElement(ghostEdge);
            }

            if (wasPanned)
            {
                if (!resetPositionOnPan || _didConnect)
                {
                    Vector3 p = GraphView.contentViewContainer.transform.position;
                    Vector3 s = GraphView.contentViewContainer.transform.scale;
                    GraphView.UpdateViewTransform(p, s);
                }
            }

            if (panSchedule != null)
                panSchedule.Pause();

            if (ghostEdge != null)
            {
                ghostEdge.input = null;
                ghostEdge.output = null;
            }

            if (draggedPort != null && !_didConnect)
            {
                draggedPort.portCapLit = false;
                draggedPort = null;
            }

            if (edgeCandidate != null)
            {
                edgeCandidate.SetEnabled(true);
            }

            ghostEdge = null;
            edgeCandidate = null;
        }

        public override bool HandleMouseDown(MouseDownEvent evt)
        {
            if (edgeCandidate.parent == null)
                GraphView.AddElement(edgeCandidate);

            switch (draggedPort.direction)
            {
                case Direction.Input:
                    edgeCandidate.input = draggedPort;
                    edgeCandidate.output = null;
                    break;
                case Direction.Output:
                    edgeCandidate.input = null;
                    edgeCandidate.output = draggedPort;
                    break;
            }

            edgeCandidate.candidatePosition = evt.mousePosition;
            edgeCandidate.SetEnabled(false);

            draggedPort.portCapLit = true;

            compatiblePorts = GraphView.GetCompatiblePorts(draggedPort, nodeAdapter);

            GraphView.ports.ForEach(port =>
            {
                port.OnStartEdgeDragging();
            });

            foreach (var port in compatiblePorts)
                port.highlight = true;

            edgeCandidate.UpdateEdgeControl();

            if (panSchedule == null)
            {
                panSchedule = GraphView.schedule.Execute(Pan).Every(k_PanInterval).StartingIn(k_PanInterval);
                panSchedule.Pause();
            }
            wasPanned = false;

            edgeCandidate.layer = Int32.MaxValue;

            return true;
        }

        internal Vector2 GetEffectivePanSpeed(Vector2 _mousePos)
        {
            Vector2 effectiveSpeed = Vector2.zero;

            if (_mousePos.x <= k_PanAreaWidth)
                effectiveSpeed.x = -(((k_PanAreaWidth - _mousePos.x) / k_PanAreaWidth) + 0.5f) * k_PanSpeed;
            else if (_mousePos.x >= GraphView.contentContainer.layout.width - k_PanAreaWidth)
                effectiveSpeed.x = (((_mousePos.x - (GraphView.contentContainer.layout.width - k_PanAreaWidth)) / k_PanAreaWidth) + 0.5f) * k_PanSpeed;

            if (_mousePos.y <= k_PanAreaWidth)
                effectiveSpeed.y = -(((k_PanAreaWidth - _mousePos.y) / k_PanAreaWidth) + 0.5f) * k_PanSpeed;
            else if (_mousePos.y >= GraphView.contentContainer.layout.height - k_PanAreaWidth)
                effectiveSpeed.y = (((_mousePos.y - (GraphView.contentContainer.layout.height - k_PanAreaWidth)) / k_PanAreaWidth) + 0.5f) * k_PanSpeed;

            effectiveSpeed = Vector2.ClampMagnitude(effectiveSpeed, k_MaxPanSpeed);

            return effectiveSpeed;
        }

        public override void HandleMouseMove(MouseMoveEvent evt)
        {
            var ve = (VisualElement)evt.target;
            Vector2 gvMousePos = ve.ChangeCoordinatesTo(GraphView.contentContainer, evt.localMousePosition);
            panDiff = GetEffectivePanSpeed(gvMousePos);

            if (panDiff != Vector3.zero)
                panSchedule.Resume();
            else
                panSchedule.Pause();

            Vector2 mousePosition = evt.mousePosition;
            edgeCandidate.candidatePosition = mousePosition;
            Port endPort = GetEndPort(mousePosition);

            if (endPort != null)
            {
                if (ghostEdge == null)
                {
                    ghostEdge = new EdgeView();
                    ghostEdge.isGhostEdge = true;
                    ghostEdge.pickingMode = PickingMode.Ignore;
                    GraphView.AddElement(ghostEdge);
                }

                if (edgeCandidate.output == null)
                {
                    ghostEdge.input = edgeCandidate.input;
                    if (ghostEdge.output != null)
                        ghostEdge.output.portCapLit = false;
                    ghostEdge.output = endPort;
                    ghostEdge.output.portCapLit = true;
                }
                else
                {
                    if (ghostEdge.input != null)
                        ghostEdge.input.portCapLit = false;
                    ghostEdge.input = endPort;
                    ghostEdge.input.portCapLit = true;
                    ghostEdge.output = edgeCandidate.output;
                }
            }
            else if (ghostEdge != null)
            {
                if (edgeCandidate.input == null)
                {
                    if (ghostEdge.input != null)
                        ghostEdge.input.portCapLit = false;
                }
                else
                {
                    if (ghostEdge.output != null)
                        ghostEdge.output.portCapLit = false;
                }
                GraphView.RemoveElement(ghostEdge);
                ghostEdge = null;
            }
        }

        private void Pan(TimerState ts)
        {
            GraphView.viewTransform.position -= panDiff;

            // Workaround to force edge to update when we pan the graph
            edgeCandidate.output = edgeCandidate.output;
            edgeCandidate.input = edgeCandidate.input;

            edgeCandidate.UpdateEdgeControl();
            wasPanned = true;
        }

        public override void HandleMouseUp(MouseUpEvent evt)
        {
            bool didConnect = false;

            Vector2 mousePosition = evt.mousePosition;

            // Reset the highlights.
            GraphView.ports.ForEach((p) =>
            {
                p.OnStopEdgeDragging();
            });

            // Clean up ghost edges.
            if (ghostEdge != null)
            {
                if (ghostEdge.input != null)
                    ghostEdge.input.portCapLit = false;
                if (ghostEdge.output != null)
                    ghostEdge.output.portCapLit = false;

                GraphView.RemoveElement(ghostEdge);
                ghostEdge.input = null;
                ghostEdge.output = null;
                ghostEdge = null;
            }

            Port endPort = GetEndPort(mousePosition);

            if (endPort == null && listener != null)
            {
                listener.OnDropOutsidePort(edgeCandidate, mousePosition);
            }

            edgeCandidate.SetEnabled(true);

            if (edgeCandidate.input != null)
                edgeCandidate.input.portCapLit = false;

            if (edgeCandidate.output != null)
                edgeCandidate.output.portCapLit = false;

            // If it is an existing valid edge then delete and notify the model (using DeleteElements()).
            if (edgeCandidate.input != null && edgeCandidate.output != null)
            {
                // Save the current input and output before deleting the edge as they will be reset
                Port oldInput = edgeCandidate.input;
                Port oldOutput = edgeCandidate.output;

                GraphView.DeleteElements(new[] { edgeCandidate });

                // Restore the previous input and output
                edgeCandidate.input = oldInput;
                edgeCandidate.output = oldOutput;
            }
            // otherwise, if it is an temporary edge then just remove it as it is not already known my the model
            else
            {
                GraphView.RemoveElement(edgeCandidate);
            }

            if (endPort != null)
            {
                if (endPort.direction == Direction.Output)
                    edgeCandidate.output = endPort;
                else
                    edgeCandidate.input = endPort;

                listener.OnDrop(GraphView, edgeCandidate);
                didConnect = true;
            }
            else
            {
                edgeCandidate.output = null;
                edgeCandidate.input = null;
            }

            edgeCandidate.ResetLayer();

            edgeCandidate = null;
            Reset(didConnect);
        }

        Rect GetPortBounds(Port _portView)
        {
            var bounds = _portView.worldBound;

            switch (_portView.orientation)
            {
                case Orientation.Horizontal:
                    if (_portView.direction == Direction.Input)
                    {
                        bounds.xMin -= kPortDetectionWidth;
                        bounds.yMin -= 10;
                    }
                    else
                    {
                        bounds.xMax += kPortDetectionWidth;
                        bounds.yMax += 10;
                    }
                    break;
                case Orientation.Vertical:
                    if (_portView.direction == Direction.Input)
                        bounds.yMin -= kPortDetectionWidth;
                    else
                        bounds.yMax += kPortDetectionWidth;
                    break;
            }

            return bounds;
        }

        Port GetEndPort(Vector2 _mousePosition)
        {
            Port bestPort = null;
            float bestDistance = 1e20f;

            foreach (var port in compatiblePorts)
            {
                Rect bounds = GetPortBounds(port);

                float distance = Vector2.Distance(port.worldBound.position, _mousePosition);

                if (bounds.Contains(_mousePosition) && distance < bestDistance)
                {
                    bestPort = port;
                    bestDistance = distance;
                }
            }
            return bestPort;
        }
    }
}