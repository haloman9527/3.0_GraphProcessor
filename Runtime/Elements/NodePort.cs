using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System;
using CZToolKit.Core;

namespace CZToolKit.GraphProcessor
{
    [Serializable]
    /// <summary> Runtime class that stores all info about one port that is needed for the processing </summary>
    public class NodePort
    {
        #region 静态方法
        /// <summary> 接口兼容性查询 </summary>
        public static bool IsCompatible(NodePort port1, NodePort port2)
        {
            bool Compatible(NodePort portA, NodePort portB)
            {
                if (portA == null || portB == null) return false;
                if (portA == portB || portA.Owner == portB.Owner)
                    return false;

                if (portA.Direction == portB.Direction)
                    return false;

                if (portA.TypeConstraint == PortTypeConstraint.None || portB.TypeConstraint == PortTypeConstraint.None) return true;
                if (portA.TypeConstraint == PortTypeConstraint.Inherited && portA.DisplayType.IsAssignableFrom(portB.DisplayType)) return true;
                if (portA.TypeConstraint == PortTypeConstraint.Strict && portA.DisplayType == portB.DisplayType) return true;
                return false;
            }

            return Compatible(port1, port2) && Compatible(port2, port1);
        }
        #endregion

        [SerializeField] BaseGraph graph;
        [SerializeField] string ownerGUID;
        [SerializeField] string fieldName;
        [SerializeField] string typeQualifiedName;
        [SerializeField] Type dataType;
        [SerializeField] bool isMulti = false;
        [SerializeField] PortDirection direction = PortDirection.Input;
        [SerializeField] PortTypeConstraint typeConstraint = PortTypeConstraint.Inherited;
        [SerializeField] List<string> edgeGUIDs = new List<string>();

        public BaseNode Owner { get { graph.Nodes.TryGetValue(ownerGUID, out BaseNode node); return node; } set { graph = value.Owner; ownerGUID = value.GUID; } }
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
        public int ConnectionCount { get { return edgeGUIDs.Count; } }
        public bool IsConnected { get { return ConnectionCount != 0; } }
        public List<string> EdgeGUIDS { get { return edgeGUIDs; } }

        public NodePort() { }

        public NodePort(FieldInfo _fieldInfo)
        {
            fieldName = _fieldInfo.Name;

            if (AttributeCache.TryGetFieldAttribute(_fieldInfo.DeclaringType, _fieldInfo.Name, out PortAttribute attribute))
            {
                isMulti = attribute.IsMulti;
                direction = attribute.Direction;
                typeConstraint = attribute.TypeConstraint;
            }

            if (AttributeCache.TryGetFieldAttribute(_fieldInfo.DeclaringType, _fieldInfo.Name, out PortTypeAttribute typeAttribute))
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
            Owner.Execute(this);
        }

        public IEnumerable<NodePort> GetConnections()
        {
            foreach (var edge in GetEdges())
            {
                yield return direction == PortDirection.Input ? edge.OutputPort : edge.InputPort;
            }
        }

        public bool TryGetConnectValue<T>(ref T _value)
        {
            NodePort port = Connection;
            if (port == null) return false;
            return port.TryGetValue(ref _value);
        }

        public IEnumerable<SerializableEdge> GetEdges()
        {
            foreach (string edge in edgeGUIDs)
            {
                yield return GetEdge(edge);
            }
        }

        public SerializableEdge GetEdge(int i)
        {
            if (graph.Edges.TryGetValue(edgeGUIDs[i], out SerializableEdge edge)) return edge;
            return null;
        }

        public SerializableEdge GetEdge(string edgeGUID)
        {
            if (graph.Edges.TryGetValue(edgeGUID, out SerializableEdge edge)) return edge;
            return null;
        }

        public void ConnectEdge(SerializableEdge _edge)
        {
            if (!edgeGUIDs.Contains(_edge.GUID))
                edgeGUIDs.Add(_edge.GUID);
        }

        /// <summary> 第二个参数是中间值 </summary>
        public void SortEdge(Func<string, string, bool> _comparison)
        {
            edgeGUIDs.QuickSort(_comparison);
        }

        public void DisconnectEdge(string _edgeGUID)
        {
            if (edgeGUIDs.Contains(_edgeGUID))
                edgeGUIDs.Remove(_edgeGUID);
        }


        public void DisconnectEdge(SerializableEdge _edge)
        {
            DisconnectEdge(_edge.GUID);
        }
    }
}