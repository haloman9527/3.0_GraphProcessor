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

using Atom;
using System;
using System.Collections.Generic;

namespace Atom.GraphProcessor
{
    [ViewModel(typeof(BaseConnection))]
    public class BaseConnectionProcessor : ViewModel, IGraphElementProcessor
    {
        /// <summary>
        /// 数据
        /// </summary>
        private BaseConnection m_Model; 
        
        /// <summary>
        /// 数据类型
        /// </summary>
        private Type m_ModelType;

        /// <summary>
        /// 所在Graph
        /// </summary>
        private BaseGraphProcessor m_Owner;
        
        /// <summary>
        /// 起点Port
        /// </summary>
        private PortProcessor m_From;
        
        /// <summary>
        /// 终点Port
        /// </summary>
        private PortProcessor m_To;

        public BaseConnectionProcessor(BaseConnection model)
        {
            this.m_Model = model;
            this.m_ModelType = model.GetType();
        }

        public BaseConnection Model
        {
            get { return m_Model; }
        }

        public Type ModelType
        {
            get { return m_ModelType; }
        }

        object IGraphElementProcessor.Model
        {
            get { return m_Model; }
        }

        Type IGraphElementProcessor.ModelType
        {
            get { return m_ModelType; }
        }

        public long FromNodeID
        {
            get { return Model.fromNode; }
        }

        public long ToNodeID
        {
            get { return Model.toNode; }
        }

        public string FromPortName
        {
            get { return Model.fromPort; }
        }

        public string ToPortName
        {
            get { return Model.toPort; }
        }

        public BaseNodeProcessor FromNode
        {
            get { return m_From.Owner; }
        }

        public PortProcessor FromPort
        {
            get { return m_From; }
        }

        public BaseNodeProcessor ToNode
        {
            get { return m_To.Owner; }
        }

        public PortProcessor ToPort
        {
            get { return m_To; }
        }

        public BaseGraphProcessor Owner
        {
            get => m_Owner;
            internal set => m_Owner = value;
        }

        internal void Enable()
        {
            if (Owner == null)
                throw new InvalidOperationException("Connection owner cannot be null when enabling");
                
            if (!Owner.Nodes.TryGetValue(Model.fromNode, out var fromNode))
                throw new KeyNotFoundException($"From node with ID {Model.fromNode} not found");
                
            if (!fromNode.Ports.TryGetValue(Model.fromPort, out var fromPort))
                throw new KeyNotFoundException($"From port '{Model.fromPort}' not found in node {Model.fromNode}");
                
            if (!Owner.Nodes.TryGetValue(Model.toNode, out var toNode))
                throw new KeyNotFoundException($"To node with ID {Model.toNode} not found");
                
            if (!toNode.Ports.TryGetValue(Model.toPort, out var toPort))
                throw new KeyNotFoundException($"To port '{Model.toPort}' not found in node {Model.toNode}");
            
            this.m_From = fromPort;
            this.m_To = toPort;
            OnEnabled();
        }
        
        protected virtual void OnEnabled()
        {
        }
    }

    public enum ConnectionSortMode
    {
        InPort,
        OutPort,
    }

    public static class ConnectionProcessorComparer
    {
        public static Predicate<BaseConnectionProcessor> EmptyComparer = EmptyComparerFunc;

        private static bool EmptyComparerFunc(BaseConnectionProcessor obj)
        {
            return obj == null;
        }
    }

    public class ConnectionProcessorHorizontalComparer : IComparer<BaseConnectionProcessor>
    {
        public static readonly ConnectionProcessorHorizontalComparer FromPortSortDefault = new ConnectionProcessorHorizontalComparer(ConnectionSortMode.OutPort);
        public static readonly ConnectionProcessorHorizontalComparer ToPortSortDefault = new ConnectionProcessorHorizontalComparer(ConnectionSortMode.InPort);

        private ConnectionSortMode m_mode;

        public ConnectionProcessorHorizontalComparer(ConnectionSortMode mode)
        {
            this.m_mode = mode;
        }

        public int Compare(BaseConnectionProcessor x, BaseConnectionProcessor y)
        {
            // 若需要重新排序的是input接口，则根据FromNode排序
            // 若需要重新排序的是output接口，则根据ToNode排序
            var nodeX = m_mode == ConnectionSortMode.InPort ? x.FromNode : x.ToNode;
            var nodeY = m_mode == ConnectionSortMode.InPort ? y.FromNode : y.ToNode;

            // 则使用y坐标比较排序
            // 遵循从上到下
            if (nodeX.Position.y < nodeY.Position.y)
                return -1;
            if (nodeX.Position.y > nodeY.Position.y)
                return 1;

            // 若节点的y坐标相同，则使用x坐标比较排序
            // 遵循从左到右
            if (nodeX.Position.x < nodeY.Position.x)
                return -1;
            if (nodeX.Position.x > nodeY.Position.x)
                return 1;

            return 0;
        }
    }

    public class ConnectionProcessorVerticalComparer : IComparer<BaseConnectionProcessor>
    {
        public static readonly ConnectionProcessorVerticalComparer InPortSortDefault = new ConnectionProcessorVerticalComparer(ConnectionSortMode.InPort);
        public static readonly ConnectionProcessorVerticalComparer OutPortSortDefault = new ConnectionProcessorVerticalComparer(ConnectionSortMode.OutPort);

        private ConnectionSortMode m_mode;

        public ConnectionProcessorVerticalComparer(ConnectionSortMode mode)
        {
            this.m_mode = mode;
        }

        public int Compare(BaseConnectionProcessor x, BaseConnectionProcessor y)
        {
            // 若需要重新排序的是input接口，则根据FromNode排序
            // 若需要重新排序的是output接口，则根据ToNode排序
            var nodeX = m_mode == ConnectionSortMode.InPort ? x.FromNode : x.ToNode;
            var nodeY = m_mode == ConnectionSortMode.InPort ? y.FromNode : y.ToNode;

            // 则使用x坐标比较排序
            // 遵循从左到右
            if (nodeX.Position.x < nodeY.Position.x)
                return -1;
            if (nodeX.Position.x > nodeY.Position.x)
                return 1;

            // 若节点的x坐标相同，则使用y坐标比较排序
            // 遵循从上到下
            if (nodeX.Position.y < nodeY.Position.y)
                return -1;
            if (nodeX.Position.y > nodeY.Position.y)
                return 1;

            return 0;
        }
    }
}