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
using CZToolKit.Core.ViewModel;
using System;

namespace CZToolKit.GraphProcessor
{
    [ViewModel(typeof(BaseConnection))]
    public class BaseConnectionVM : ViewModel
    {
        #region Fields
        [NonSerialized] BasePortVM from;
        [NonSerialized] BasePortVM to;
        #endregion

        #region Properties
        public BaseConnection Model
        {
            get;
        }
        public Type ModelType
        {
            get;
        }
        public BaseGraphVM Owner
        {
            get;
            internal set;
        }
        public string FromNodeGUID
        {
            get { return Model.fromNode; }
        }
        public string ToNodeGUID
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
        public BaseNodeVM FromNode
        {
            get { return from.Owner; }
        }
        public BasePortVM FromPort
        {
            get { return from; }
        }
        public BaseNodeVM ToNode
        {
            get { return to.Owner; }
        }
        public BasePortVM ToPort
        {
            get { return to; }
        }
        #endregion

        public BaseConnectionVM(BaseConnection model)
        {
            Model = model;
            ModelType = model.GetType();
        }

        internal void Enable(BaseGraphVM graph)
        {
            Owner = graph;
            this.from = Owner.Nodes[Model.fromNode].Ports[Model.fromPort];
            this.to = Owner.Nodes[Model.toNode].Ports[Model.toPort];
            OnEnabled();
        }

        /// <summary> 重定向 </summary>
        public void Redirect(BasePortVM from, BasePortVM to)
        {
            Model.fromNode = from.Owner.GUID;
            Model.fromPort = from.Name;
            Model.toNode = to.Owner.GUID;
            Model.toPort = to.Name;
            Enable(Owner);
        }

        #region Overrides
        protected virtual void OnEnabled() { }

        protected virtual void OnRedirection() { }
        #endregion
    }
}
