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
 *  Github: https://github.com/haloman9527
 *  Blog: https://www.haloman.net/
 *
 */

#endregion

using System;
using System.Collections.Generic;

namespace Atom.GraphProcessor
{
    public partial class BaseGraphProcessor
    {
        #region Fields

        private Dictionary<long, BaseNodeProcessor> nodes;

        public event Action<BaseNodeProcessor> OnNodeAdded;
        public event Action<BaseNodeProcessor> OnNodeRemoved;

        #endregion

        #region Properties
        
        public IReadOnlyDictionary<long, BaseNodeProcessor> Nodes => nodes;

        #endregion

        private void BeginInitNodes()
        {
            this.nodes = new Dictionary<long, BaseNodeProcessor>(Model.nodes.Count);
            for (var index = 0; index < Model.nodes.Count; index++)
            {
                var node = Model.nodes[index];
                if (node == null)
                    continue;
                var nodeProcessor = (BaseNodeProcessor)ViewModelFactory.ProduceViewModel(node);
                nodeProcessor.Owner = this;
                nodeProcessor.Index = index;
                nodes.Add(node.id, nodeProcessor);
            }
        }

        private void EndInitNodes()
        {
            foreach (var node in nodes.Values)
            {
                node.Enable();
            }
        }

        #region API

        public BaseNodeProcessor AddNode<T>(InternalVector2Int position) where T : BaseNode, new()
        {
            return AddNode(TypeCache<T>.TYPE, position);
        }

        public BaseNodeProcessor AddNode(Type nodeType, InternalVector2Int position)
        {
            var nodeVM = NewNode(nodeType, position);
            AddNode(nodeVM);
            return nodeVM;
        }

        public BaseNodeProcessor AddNode(BaseNode nodeData)
        {
            var nodeVM = ViewModelFactory.ProduceViewModel(nodeData) as BaseNodeProcessor;
            AddNode(nodeVM);
            return nodeVM;
        }

        public void AddNode(BaseNodeProcessor node)
        {
            nodes.Add(node.ID, node);
            model.nodes.Add(node.Model);
            node.Owner = this;
            node.Index = model.nodes.Count - 1;
            node.Enable();
            OnNodeAdded?.Invoke(node);
        }

        public void RemoveNodeByID(int id)
        {
            RemoveNode(Nodes[id]);
        }

        public void RemoveNode(BaseNodeProcessor node)
        {
            if (node.Owner != this)
                throw new NullReferenceException("节点不是此Graph中");

            if (groups.NodeGroupMap.TryGetValue(node.ID, out var group))
                groups.RemoveNodeFromGroup(node);

            Disconnect(node);
            nodes.Remove(node.ID);
            model.nodes.Remove(node.Model);
            node.Disable();
            for (int index = 0; index < model.nodes.Count; index++)
            {
                var nodeData = model.nodes[index];
                nodes[nodeData.id].Index = index;
            }
            OnNodeRemoved?.Invoke(node);
        }

        public virtual BaseNodeProcessor NewNode(Type nodeType, InternalVector2Int position)
        {
            var node = Activator.CreateInstance(nodeType) as BaseNode;
            node.id = GraphProcessorUtil.GenerateId();
            node.position = position;
            return ViewModelFactory.ProduceViewModel(node) as BaseNodeProcessor;
        }

        public virtual BaseNodeProcessor NewNode<TNode>(InternalVector2Int position) where TNode : BaseNode, new()
        {
            var node = new TNode()
            {
                id = GraphProcessorUtil.GenerateId(),
                position = position
            };
            return ViewModelFactory.ProduceViewModel(node) as BaseNodeProcessor;
        }

        #endregion
    }
}