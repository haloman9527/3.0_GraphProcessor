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
    public partial class BaseConnection : ViewModel
    {
        #region Fields
        [NonSerialized] internal INode fromNode;
        [NonSerialized] internal INode toNode;
        #endregion

        #region Properties
        public IGraph Owner { get; internal set; }
        public string FromNodeGUID { get { return from; } }
        public string ToNodeGUID { get { return to; } }
        public string FromPortName { get { return fromPortName; } }
        public string ToPortName { get { return toPortName; } }
        public INode FromNode { get { return fromNode; } }
        public INode ToNode { get { return toNode; } }
        #endregion

        public BaseConnection() { }

        internal void Enable(IGraph graph)
        {
            Owner = graph;
            OnEnabled();
        }

        /// <summary> 重定向 </summary>
        internal void Redirect(INode from, string fromPortName, INode to, string toPortName)
        {
            this.fromNode = from;
            this.from = from.GUID;
            this.fromPortName = fromPortName;

            this.toNode = to;
            this.to = to.GUID;
            this.toPortName = toPortName;

            Enable(Owner);
        }

        #region Overrides
        protected virtual void OnEnabled() { }
        #endregion
    }
}
