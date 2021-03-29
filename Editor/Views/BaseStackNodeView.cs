using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

namespace GraphProcessor.Editors
{
    public class BaseStackNodeView : StackNode
    {
        const string styleSheet = "GraphProcessorStyles/BaseStackNodeView";
        public delegate void ReorderNodeAction(BaseNodeView nodeView, int oldIndex, int newIndex);
        public static StyleSheet stackStyle;
        public static StyleSheet StackStyle
        {
            get
            {
                if (stackStyle == null)
                    stackStyle = Resources.Load<StyleSheet>(styleSheet);
                return stackStyle;
            }
        }

        protected BaseGraphView owner;
        protected internal BaseStackNode stackNode;

        public event ReorderNodeAction onNodeReordered;

        public virtual void Initialize(BaseGraphView _graphView, BaseStackNode _stackNode)
        {
            styleSheets.Add(StackStyle);
            owner = _graphView;
            stackNode = _stackNode;
            headerContainer.Add(new Label(stackNode.title));
            SetPosition(new Rect(stackNode.position, Vector2.one));
            this.Q("stackSeparatorContainer").style.visibility = Visibility.Hidden;
            InitializeInnerNodes();
        }

        void InitializeInnerNodes()
        {
            int i = 0;
            foreach (var nodeGUID in stackNode.nodeGUIDs)
            {
                if (owner.GraphData.Nodes.TryGetValue(nodeGUID, out BaseNode node))
                {
                    BaseNodeView nodeView = owner.NodeViews[nodeGUID];
                    nodeView.AddToClassList("stack-child__" + i);
                    AddElement(nodeView);
                    i++;
                }
            }
        }

        public override void SetPosition(Rect newPos)
        {
            base.SetPosition(newPos);
            stackNode.position = newPos.position;
        }

        protected override bool AcceptsElement(GraphElement element, ref int proposedIndex, int maxIndex)
        {
            if (element is ParameterNodeView || element is RelayNodeView)
                return false;
            bool accept = base.AcceptsElement(element, ref proposedIndex, maxIndex);

            if (accept && element is BaseNodeView nodeView)
            {
                var index = Mathf.Clamp(proposedIndex, 0, stackNode.nodeGUIDs.Count - 1);

                int oldIndex = stackNode.nodeGUIDs.FindIndex(g => g == nodeView.NodeData.GUID);
                if (oldIndex != -1)
                {
                    stackNode.nodeGUIDs.Remove(nodeView.NodeData.GUID);
                    if (oldIndex != index)
                        onNodeReordered?.Invoke(nodeView, oldIndex, index);
                }

                stackNode.nodeGUIDs.Insert(index, nodeView.NodeData.GUID);
            }

            return accept;
        }

        public override bool DragLeave(DragLeaveEvent evt, IEnumerable<ISelectable> selection, IDropTarget leftTarget, ISelection dragSource)
        {
            foreach (var elem in selection)
            {
                if (elem is BaseNodeView nodeView)
                    stackNode.nodeGUIDs.Remove(nodeView.NodeData.GUID);
            }
            return base.DragLeave(evt, selection, leftTarget, dragSource);
        }
    }
}