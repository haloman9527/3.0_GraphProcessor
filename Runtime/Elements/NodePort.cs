using CZToolKit.Core;
using UnityEngine;
using System;
using System.Reflection;
using System.Collections.Generic;

namespace CZToolKit.GraphProcessor
{
    [Serializable]
    public class NodePort : IGraphElement
    {
        #region 静态方法
        /// <summary> 接口兼容性查询 </summary>
        public static bool IsCompatible(NodePort _port1, NodePort _port2)
        {
            if (_port1 == null || _port2 == null) return false;
            if (_port1 == _port2 || _port1.Owner == _port2.Owner)
                return false;

            if (_port1.Direction == _port2.Direction)
                return false;

            bool Compatible(NodePort portA, NodePort portB)
            {
                if (portA.TypeConstraint == PortTypeConstraint.None) return true;
                if (portA.TypeConstraint == PortTypeConstraint.Inherited && portA.DisplayType.IsAssignableFrom(portB.DisplayType)) return true;
                if (portA.TypeConstraint == PortTypeConstraint.Strict && portA.DisplayType == portB.DisplayType) return true;
                return false;
            }

            return Compatible(_port1, _port2) && Compatible(_port2, _port1);
        }
        #endregion

        [NonSerialized] IGraph graph;
        [SerializeField] string ownerGUID;
        [SerializeField] string fieldName;
        [SerializeField] string typeQualifiedName;
        [SerializeField] Type dataType;
        [SerializeField] bool isMulti = false;
        [SerializeField] PortDirection direction = PortDirection.Input;
        [SerializeField] PortTypeConstraint typeConstraint = PortTypeConstraint.Inherited;
        [SerializeField] List<string> edgeGUIDs = new List<string>();

        public BaseNode Owner { get { graph.NodesGUIDMapping.TryGetValue(ownerGUID, out BaseNode node); return node; } set { graph = value.Owner; ownerGUID = value.GUID; } }
        public string FieldName { get { return fieldName; } set { fieldName = value; } }
        public bool IsMulti { get { return isMulti; } set { isMulti = value; } }
        public PortDirection Direction { get { return direction; } set { direction = value; } }
        public PortTypeConstraint TypeConstraint { get { return typeConstraint; } set { typeConstraint = value; } }
        public Type DisplayType
        {
            get
            {
                if (dataType == null && !string.IsNullOrEmpty(typeQualifiedName)) dataType = Type.GetType(typeQualifiedName, false);
                return dataType;
            }
            set
            {
                dataType = value;
                if (value != null) typeQualifiedName = value.AssemblyQualifiedName;
            }
        }
        /// <summary> 第一个不为空的连接 </summary>
        public NodePort Connection
        {
            get
            {
                foreach (SerializableEdge edge in GetEdges())
                {
                    if (edge == null) continue;
                    if (edge.InputNodeGUID == ownerGUID) return edge.OutputPort;
                    else return edge.InputPort;
                }

                return null;
            }
        }
        public List<string> EdgeGUIDS { get { return edgeGUIDs; } }
        public int ConnectionCount { get { return edgeGUIDs.Count; } }
        public bool IsConnected { get { return ConnectionCount != 0; } }

        public NodePort() { }

        public NodePort(FieldInfo _fieldInfo, PortAttribute _portAttribute)
        {
            fieldName = _fieldInfo.Name;
            isMulti = _portAttribute.IsMulti;
            direction = _portAttribute.Direction;
            typeConstraint = _portAttribute.TypeConstraint;

            if (Utility_Attribute.TryGetFieldAttribute(_fieldInfo.DeclaringType, _fieldInfo.Name, out PortTypeAttribute typeAttribute))
                DisplayType = typeAttribute.PortType;
            else
                DisplayType = _fieldInfo.FieldType;
        }

        public NodePort(NodePort port, BaseNode node)
        {
            Owner = node;
            fieldName = port.fieldName;
            direction = port.direction;
            isMulti = port.IsMulti;
            typeConstraint = port.typeConstraint;

            DisplayType = Owner.PortDynamicType(FieldName);
            if (DisplayType == null)
                DisplayType = port.DisplayType;
        }

        public void Reload(NodePort port)
        {
            fieldName = port.fieldName;
            direction = port.direction;
            isMulti = port.isMulti;
            typeConstraint = port.typeConstraint;
            Type tempDisplayType = Owner.PortDynamicType(FieldName);
            if (tempDisplayType != null)
                DisplayType = tempDisplayType;
            else
                DisplayType = port.DisplayType;
        }

        public bool TryGetValue<T>(ref T _value)
        {
            return Owner.GetValue(this, ref _value);
        }

        public void Execute(params object[] _params)
        {
            Owner.Execute(this, _params);
        }

        /// <summary> 返回所有连接 </summary>
        /// <returns></returns>
        public IEnumerable<SerializableEdge> GetEdges()
        {
            foreach (string edge in edgeGUIDs)
            {
                yield return GetEdge(edge);
            }
        }

        /// <summary> 根据EdgeGUID获取连接的端口 </summary>
        /// <param name="_edgeGUID"></param>
        /// <returns></returns>
        public NodePort GetConnection(int _edgeGUID)
        {
            SerializableEdge edge = GetEdge(_edgeGUID);
            if (edge == null)
                return null;
            return Direction == PortDirection.Input ? edge.OutputPort : edge.InputPort;
        }

        /// <summary> 返回所有连接的远程端口 </summary>
        /// <returns></returns>
        public IEnumerable<NodePort> GetConnections()
        {
            foreach (var edge in GetEdges())
            {
                yield return direction == PortDirection.Input ? edge.OutputPort : edge.InputPort;
            }
        }

        /// <summary> 尝试获取第一个连接的远程端口的值 </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="_value"></param>
        /// <returns></returns>
        public bool TryGetConnectValue<T>(ref T _value)
        {
            NodePort port = Connection;
            if (port == null) return false;
            return port.TryGetValue(ref _value);
        }

        /// <summary> 返回所有连接的远程端口的值 </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IEnumerable<T> GetConnectValues<T>()
        {
            foreach (var port in GetConnections())
            {
                T value = default;
                if (port.TryGetValue(ref value))
                    yield return value;
            }
        }

        /// <summary> 第二个参数是中间值(快排) </summary>
        public void SortEdge(Func<string, string, bool> _comparison)
        {
            edgeGUIDs.QuickSort(_comparison);
        }

        /// <summary> 根据索引返回连接 </summary>
        /// <param name="_index"></param>
        /// <returns></returns>
        public SerializableEdge GetEdge(int _index)
        {
            if (graph.EdgesGUIDMapping.TryGetValue(edgeGUIDs[_index], out SerializableEdge edge)) return edge;
            return null;
        }

        /// <summary> 根据GUID返回连接 </summary>
        /// <param name="_edgeGUID"></param>
        /// <returns></returns>
        public SerializableEdge GetEdge(string _edgeGUID)
        {
            if (graph.EdgesGUIDMapping.TryGetValue(_edgeGUID, out SerializableEdge edge)) return edge;
            return null;
        }

        public void ConnectToEdge(SerializableEdge _edge)
        {
            if (!edgeGUIDs.Contains(_edge.GUID))
                edgeGUIDs.Add(_edge.GUID);
        }

        public void DisconnectToEdge(string _edgeGUID)
        {
            if (edgeGUIDs.Contains(_edgeGUID))
                edgeGUIDs.Remove(_edgeGUID);
        }

        public void DisconnectToEdge(SerializableEdge _edge)
        {
            DisconnectToEdge(_edge.GUID);
        }
    }
}