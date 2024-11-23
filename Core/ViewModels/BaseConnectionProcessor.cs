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

using Jiange;
using System;
using System.Collections.Generic;

namespace Jiange.GraphProcessor
{
    [ViewModel(typeof(BaseConnection))]
    public class BaseConnectionProcessor : ViewModel, IGraphElementProcessor
    {
        #region Fields

        [NonSerialized] BasePortProcessor from;
        [NonSerialized] BasePortProcessor to;

        #endregion

        #region Properties

        public BaseConnection Model { get; }

        public Type ModelType { get; }

        public BaseGraphProcessor Owner { get; internal set; }

        public int FromNodeID
        {
            get { return Model.fromNode; }
        }

        public int ToNodeID
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
            get { return from.Owner; }
        }

        public BasePortProcessor FromPort
        {
            get { return from; }
        }

        public BaseNodeProcessor ToNode
        {
            get { return to.Owner; }
        }

        public BasePortProcessor ToPort
        {
            get { return to; }
        }

        #endregion

        public BaseConnectionProcessor(BaseConnection model)
        {
            Model = model;
            ModelType = model.GetType();
        }

        public T ModelAs<T>() where T : BaseConnection
        {
            return Model as T;
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
        FromPort,
        ToPort
    }

    public class ConnectionProcessorHorizontalComparer : IComparer<BaseConnectionProcessor>
    {
        public static readonly ConnectionProcessorHorizontalComparer FromPortSortDefault = new ConnectionProcessorHorizontalComparer(ConnectionSortMode.FromPort);
        public static readonly ConnectionProcessorHorizontalComparer ToPortSortDefault = new ConnectionProcessorHorizontalComparer(ConnectionSortMode.ToPort);

        private ConnectionSortMode m_mode;

        public ConnectionProcessorHorizontalComparer(ConnectionSortMode mode)
        {
            this.m_mode = mode;
        }

        public int Compare(BaseConnectionProcessor x, BaseConnectionProcessor y)
        {
            // 若需要重新排序的是input接口，则根据FromNode排序
            // 若需要重新排序的是output接口，则根据ToNode排序
            var nodeX = m_mode == ConnectionSortMode.FromPort ? x.ToNode : x.FromNode;
            var nodeY = m_mode == ConnectionSortMode.FromPort ? y.ToNode : y.FromNode;

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
        public static readonly ConnectionProcessorVerticalComparer FromPortSortDefault = new ConnectionProcessorVerticalComparer(ConnectionSortMode.FromPort);
        public static readonly ConnectionProcessorVerticalComparer ToPortSortDefault = new ConnectionProcessorVerticalComparer(ConnectionSortMode.ToPort);

        private ConnectionSortMode m_mode;

        public ConnectionProcessorVerticalComparer(ConnectionSortMode mode)
        {
            this.m_mode = mode;
        }

        public int Compare(BaseConnectionProcessor x, BaseConnectionProcessor y)
        {
            // 若需要重新排序的是input接口，则根据FromNode排序
            // 若需要重新排序的是output接口，则根据ToNode排序
            var nodeX = m_mode == ConnectionSortMode.FromPort ? x.ToNode : x.FromNode;
            var nodeY = m_mode == ConnectionSortMode.FromPort ? y.ToNode : y.FromNode;

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