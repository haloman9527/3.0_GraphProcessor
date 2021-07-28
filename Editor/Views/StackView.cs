#region 注 释
/***
 *
 *  Title:
 *  
 *  Description:
 *  
 *  Date:
 *  Version:
 *  Writer: 半只龙虾人
 *  Github: https://github.com/HalfLobsterMan
 *  Blog: https://www.crosshair.top/
 *
 */
#endregion
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

namespace CZToolKit.GraphProcessor.Editors
{
    public sealed class StackView : StackNode
    {
        public Label TitleLabel { get; }
        CommandDispatcher CommandDispatcher { get; set; }

        public BaseGraphView Owner { get; private set; }

        public StackPanel Model { get; private set; }

        public StackView()
        {
            TitleLabel = new Label();
            headerContainer.Add(TitleLabel);
            this.Q("stackSeparatorContainer").style.visibility = Visibility.Hidden;
        }

        public void SetUp(StackPanel _stack, CommandDispatcher _commandDispatcher, BaseGraphView _graphView)
        {
            Model = _stack;
            CommandDispatcher = _commandDispatcher;
            Owner = _graphView;

            BindingProperties();

            InitializeInnerNodes();
        }

        void BindingProperties()
        {
            Model.BindingProperty<string>(nameof(Model.Title), v =>
            {
                title = v;
            });

            Model.BindingProperty<Vector2>(nameof(Model.Position), v =>
            {
                base.SetPosition(new Rect(v, Vector2.one));
            });

            Model.onNodeAdded += guid =>
            {

            };

            Model.onNodeRemoved += guid =>
            {

            };

            Model.onNodeInserted += (index, guid) =>
            {

            };
        }

        void InitializeInnerNodes()
        {
            int i = 0;
            foreach (var nodeGUID in Model.NodeGUIDs)
            {
                if (Owner.Model.Nodes.TryGetValue(nodeGUID, out BaseNode node))
                {
                    BaseNodeView nodeView = Owner.NodeViews[nodeGUID];
                    nodeView.AddToClassList("stack-child__" + i);
                    AddElement(nodeView);
                    i++;
                }
            }
        }

        public override void SetPosition(Rect newPos)
        {
            Model.Position = newPos.position;
        }

        protected override bool AcceptsElement(GraphElement _element, ref int proposedIndex, int maxIndex)
        {
            if (_element is ParameterNodeView || _element is RelayNodeView)
                return false;
            bool accept = base.AcceptsElement(_element, ref proposedIndex, maxIndex);

            if (accept && _element is BaseNodeView nodeView)
            {
                int index = Mathf.Clamp(proposedIndex, 0, Model.NodeGUIDs.Count - 1);

                int oldIndex = Model.FindIndex(g => g == nodeView.Model.GUID);
                if (oldIndex != -1)
                    Model.Remove(nodeView.Model.GUID);

                Model.Insert(index, nodeView.Model.GUID);
            }

            return accept;
        }

        public override bool DragLeave(DragLeaveEvent evt, IEnumerable<ISelectable> selection, IDropTarget leftTarget, ISelection dragSource)
        {
            foreach (var elem in selection)
            {
                if (elem is BaseNodeView nodeView)
                    Model.Remove(nodeView.Model.GUID);
            }
            return base.DragLeave(evt, selection, leftTarget, dragSource);
        }
    }
}