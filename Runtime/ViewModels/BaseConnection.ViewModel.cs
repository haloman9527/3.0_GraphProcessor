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
using System;

namespace CZToolKit.GraphProcessor
{
    public partial class BaseConnection : IntegratedViewModel, IGraphElement
    {
        #region 静态方法
        public static T CreateNew<T>(BaseNode from, string fromPortName, BaseNode to, string toPortName) where T : BaseConnection
        {
            return CreateNew(typeof(T), from, fromPortName, to, toPortName) as T;
        }

        public static BaseConnection CreateNew(Type type, BaseNode from, string fromPortName, BaseNode to, string toPortName)
        {
            var connection = Activator.CreateInstance(type) as BaseConnection;
            connection.from = from.GUID;
            connection.fromPortName = fromPortName;
            connection.to = to.GUID;
            connection.toPortName = toPortName;
            return connection;
        }
        #endregion

        [NonSerialized] BaseGraph owner;
        [NonSerialized] BaseNode fromNode;
        [NonSerialized] BaseNode toNode;

        public string FromNodeGUID { get { return from; } }
        public string ToNodeGUID { get { return to; } }
        public string FromPortName { get { return fromPortName; } }
        public string ToPortName { get { return toPortName; } }

        public BaseNode FromNode { get { return fromNode; } }
        public BaseNode ToNode { get { return toNode; } }

        public void Enable(BaseGraph graph)
        {
            owner = graph;
            owner.Nodes.TryGetValue(FromNodeGUID, out fromNode);
            owner.Nodes.TryGetValue(ToNodeGUID, out toNode);
        }
        protected override void BindProperties() { }
    }
}
