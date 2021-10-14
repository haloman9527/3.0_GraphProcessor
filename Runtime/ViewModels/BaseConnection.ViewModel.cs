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
    public partial class BaseConnection : IntegratedViewModel
    {
        #region 静态方法
        /// <summary> 根据T创建一个节点，并设置位置 </summary>
        public static T CreateNew<T>(BaseNode from, string fromSlotName, BaseNode to, string toSlotName) where T : BaseConnection
        {
            return CreateNew(typeof(T), from, fromSlotName, to, toSlotName) as T;
        }

        /// <summary> 根据_type创建一个节点，并设置位置 </summary>
        public static BaseConnection CreateNew(Type type, BaseNode from, string fromSlotName, BaseNode to, string toSlotName)
        {
            if (!type.IsSubclassOf(typeof(BaseConnection)))
                return null;
            var connection = Activator.CreateInstance(type) as BaseConnection;
            connection.from = from.GUID;
            connection.fromSlotName = fromSlotName;
            connection.to = to.GUID;
            connection.toSlotName = toSlotName;
            return connection;
        }
        #endregion

        [NonSerialized] BaseGraph owner;

        public string FromNodeGUID { get { return from; } }
        public string ToNodeGUID { get { return to; } }
        public string FromSlotName { get { return fromSlotName; } }
        public string ToSlotName { get { return toSlotName; } }

        public BaseNode FromNode { get { owner.Nodes.TryGetValue(FromNodeGUID, out BaseNode node); return node; } }
        public BaseNode ToNode { get { owner.Nodes.TryGetValue(ToNodeGUID, out BaseNode node); return node; } }

        public void Enable(BaseGraph graph) { owner = graph; }
        public override void InitializeBindableProperties() { }
    }
}
