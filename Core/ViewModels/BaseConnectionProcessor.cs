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
        #region Fields

        private BaseConnection model;
        private Type modelType;
        [NonSerialized] private BasePortProcessor from;
        [NonSerialized] private BasePortProcessor to;

        #endregion

        #region Properties

        public BaseConnection Model => model;

        public Type ModelType => modelType;
        
        object IGraphElementProcessor.Model => model;
        
        Type IGraphElementProcessor.ModelType => modelType;

        public int FromNodeID => Model.fromNode;

        public int ToNodeID => Model.toNode;

        public string FromPortName => Model.fromPort;

        public string ToPortName => Model.toPort;

        public BaseNodeProcessor FromNode => from.Owner;

        public BasePortProcessor FromPort => from;

        public BaseNodeProcessor ToNode => to.Owner;

        public BasePortProcessor ToPort => to;

        public BaseGraphProcessor Owner { get; internal set; }

        #endregion

        public BaseConnectionProcessor(BaseConnection model)
        {
            this.model = model;
            this.modelType = model.GetType();
        }

        internal void Enable()
        {
            this.from = Owner.Nodes[Model.fromNode].Ports[Model.fromPort];
            this.to = Owner.Nodes[Model.toNode].Ports[Model.toPort];
            OnEnabled();
        }

        /// <summary> 重定向 </summary>
        internal void Redirect(BasePortProcessor from, BasePortProcessor to)
        {
            Model.fromNode = from.Owner.ID;
            Model.fromPort = from.Name;
            Model.toNode = to.Owner.ID;
            Model.toPort = to.Name;
            Enable();
        }

        #region Virtual

        protected virtual void OnEnabled()
        {
        }

        #endregion
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