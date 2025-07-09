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

        private Dictionary<long, BaseNodeProcessor> m_Nodes;

        #endregion

        #region Properties
        
        public IReadOnlyDictionary<long, BaseNodeProcessor> Nodes
        {
            get { return m_Nodes; }
        }

        #endregion

        private void BeginInitNodes()
        {
            this.m_Nodes = new Dictionary<long, BaseNodeProcessor>(Model.nodes.Count);
            for (var index = 0; index < Model.nodes.Count; index++)
            {
                var node = Model.nodes[index];
                if (node == null)
                    continue;
                var nodeProcessor = (BaseNodeProcessor)ViewModelFactory.ProduceViewModel(node);
                nodeProcessor.Owner = this;
                nodeProcessor.Index = index;
                m_Nodes.Add(node.id, nodeProcessor);
            }
        }

        private void EndInitNodes()
        {
            foreach (var node in m_Nodes.Values)
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
            m_Nodes.Add(node.ID, node);
            m_Model.nodes.Add(node.Model);
            node.Owner = this;
            node.Index = m_Model.nodes.Count - 1;
            node.Enable();
            m_GraphEvents.Publish(new AddNodeEventArgs(node));
        }

        public void RemoveNode(int nodeId)
        {
            RemoveNode(Nodes[nodeId]);
        }

        public void RemoveNode(BaseNodeProcessor node)
        {
            if (node.Owner != this)
                throw new NullReferenceException("节点不是此Graph中");

            if (m_Groups.NodeGroupMap.TryGetValue(node.ID, out var group))
                m_Groups.RemoveNodeFromGroup(node);

            Disconnect(node);
            m_Nodes.Remove(node.ID);
            m_Model.nodes.Remove(node.Model);
            node.Disable();
            for (int index = 0; index < m_Model.nodes.Count; index++)
            {
                var nodeData = m_Model.nodes[index];
                m_Nodes[nodeData.id].Index = index;
            }
            m_GraphEvents.Publish(new RemoveNodeEventArgs(node));
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