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
 *  Blog: https://www.mindgear.net/
 *
 */

#endregion

using CZToolKit;
using System;

namespace CZToolKit.GraphProcessor
{
    [ViewModel(typeof(BaseConnection))]
    public class BaseConnectionProcessor : ViewModel, IGraphElementViewModel
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
}