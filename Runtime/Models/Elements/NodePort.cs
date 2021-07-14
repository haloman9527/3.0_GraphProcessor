using CZToolKit.Core;
using UnityEngine;
using System;
using System.Reflection;
using System.Collections.Generic;

namespace CZToolKit.GraphProcessor
{
    [Serializable]
    public class NodePort : BaseGraphElement
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

            if (_port1.TypeConstraint == PortTypeConstraint.None || _port2.TypeConstraint == PortTypeConstraint.None) return true;

            bool Compatible(NodePort portA, NodePort portB)
            {
                if (portA.TypeConstraint == PortTypeConstraint.Inherited && portA.DisplayType.IsAssignableFrom(portB.DisplayType)) return true;
                if (portA.TypeConstraint == PortTypeConstraint.Strict && portA.DisplayType == portB.DisplayType) return true;
                return false;
            }

            return Compatible(_port1, _port2) && Compatible(_port2, _port1);
        }
        #endregion


        #region Model
        [SerializeField] public string fieldName;
        [SerializeField] public string typeQualifiedName;
        [SerializeField] public bool multiple = false;
        [SerializeField] public PortDirection direction = PortDirection.Input;
        [SerializeField] public PortTypeConstraint typeConstraint = PortTypeConstraint.Inherited;
        [SerializeField] public List<string> edgeGUIDs = new List<string>();

        public NodePort() { }

        public NodePort(FieldInfo _fieldInfo, PortAttribute _portAttribute)
        {
            fieldName = _fieldInfo.Name;
            multiple = _portAttribute.IsMulti;
            direction = _portAttribute.Direction;
            typeConstraint = _portAttribute.TypeConstraint;

            if (Utility_Attribute.TryGetFieldAttribute(_fieldInfo.DeclaringType, _fieldInfo.Name, out PortTypeAttribute typeAttribute))
                typeQualifiedName = typeAttribute.portType.AssemblyQualifiedName;
            else
                typeQualifiedName = _fieldInfo.FieldType.AssemblyQualifiedName;
        }

        public NodePort(NodePort _port)
        {
            fieldName = _port.fieldName;
            direction = _port.direction;
            multiple = _port.multiple;
            typeConstraint = _port.typeConstraint;
            typeQualifiedName = _port.typeQualifiedName;
        }

        public void Reload(NodePort port)
        {
            fieldName = port.fieldName;
            direction = port.direction;
            multiple = port.multiple;
            typeConstraint = port.typeConstraint;
            typeQualifiedName = port.typeQualifiedName;
        }
        #endregion

        #region ViewModel
        [NonSerialized] Type dataType;

        [NonSerialized] BaseNode owner;
        public BaseNode Owner
        {
            get { return owner; }
            private set { owner = value; }
        }
        public string FieldName { get { return fieldName; } }
        public bool Multiple { get { return multiple; } }
        public PortDirection Direction { get { return direction; } }
        public PortTypeConstraint TypeConstraint { get { return typeConstraint; } }
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
                foreach (BaseEdge edge in GetEdges())
                {
                    if (edge == null) continue;
                    if (edge.InputNodeGUID == Owner.GUID)
                        return edge.OutputPort;
                    else
                        return edge.InputPort;
                }

                return null;
            }
        }
        /// <summary> 第一个不为空的节点 </summary>
        public BaseNode ConnectionNode
        {
            get
            {
                if (Connection == null) return null;
                return Connection.Owner;
            }
        }
        public List<string> EdgeGUIDs { get { return edgeGUIDs; } }
        public int ConnectionCount { get { return edgeGUIDs.Count; } }
        public bool IsConnected { get { return ConnectionCount != 0; } }
        public string PortName
        {
            get { return GetPropertyValue<string>(nameof(PortName)); }
            set { SetPropertyValue(nameof(PortName), value); }
        }
        public string Tooltip
        {
            get { return GetPropertyValue<string>(nameof(Tooltip)); }
            set { SetPropertyValue(nameof(Tooltip), value); }
        }
        public Color PortColor
        {
            get { return GetPropertyValue<Color>(nameof(PortColor)); }
            set { SetPropertyValue(nameof(PortColor), value); }
        }

        internal void Enable(BaseNode _node)
        {
            Owner = _node;
        }

        public override void InitializeBindableProperties()
        {
            SetBindableProperty(nameof(PortName), new BindableProperty<string>(UnityEditor.ObjectNames.NicifyVariableName(fieldName)));
            SetBindableProperty(nameof(Tooltip), new BindableProperty<string>());
            SetBindableProperty(nameof(PortColor), new BindableProperty<Color>());

//            if (Utility_Attribute.TryGetFieldAttribute(Owner.GetType(), fieldName, out InspectorNameAttribute inspectorName))
//                GetBindableProperty<string>(nameof(PortName)).SetValueWithoutNotify(inspectorName.displayName);
//#if UNITY_EDITOR
//            else
//                GetBindableProperty<string>(nameof(PortName)).SetValueWithoutNotify(UnityEditor.ObjectNames.NicifyVariableName(fieldName));
//#endif

            //if (Utility_Attribute.TryGetFieldAttribute(Owner.GetType(), fieldName, out TooltipAttribute tooltip))
            //    GetBindableProperty<string>(nameof(Tooltip)).SetValueWithoutNotify(tooltip.tooltip);

            //if (Utility_Attribute.TryGetFieldAttribute(Owner.GetType(), fieldName, out PortColorAttribute color))
            //    GetBindableProperty<Color>(nameof(PortColor)).SetValueWithoutNotify(color.Color);
        }

        public object GetValue()
        {
            return Owner.GetValue(this);
        }

        public void Execute(params object[] _params)
        {
            Owner.Execute(this, _params);
        }

        /// <summary> 返回所有连接 </summary>
        public IEnumerable<BaseEdge> GetEdges()
        {
            foreach (string edge in edgeGUIDs)
            {
                yield return GetEdge(edge);
            }
        }

        /// <summary> 通过EdgeGUID获取连接的远程端口 </summary>
        public NodePort GetConnection(int _edgeGUID)
        {
            BaseEdge edge = GetEdge(_edgeGUID);
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
        public object GetConnectValue()
        {
            NodePort connection = Connection;
            if (connection == null) return null;
            return connection.GetValue();
        }

        /// <summary> 返回所有连接的远程端口的值 </summary>
        public IEnumerable<object> GetConnectValues()
        {
            foreach (var port in GetConnections())
            {
                object value = port.GetValue();
                if (value != null)
                    yield return value;
            }
        }

        /// <summary> 第二个参数是中间值(快排) </summary>
        public void SortEdge(Func<string, string, bool> _comparison)
        {
            edgeGUIDs.QuickSort(_comparison);
        }

        /// <summary> 根据索引返回连接 </summary>
        public BaseEdge GetEdge(int _index)
        {
            if (Owner.Owner.Edges.TryGetValue(edgeGUIDs[_index], out BaseEdge edge)) return edge;
            return null;
        }

        /// <summary> 根据GUID返回连接 </summary>
        public BaseEdge GetEdge(string _edgeGUID)
        {
            if (Owner.Owner.Edges.TryGetValue(_edgeGUID, out BaseEdge edge)) return edge;
            return null;
        }

        public void ConnectToEdge(BaseEdge _edge)
        {
            if (!edgeGUIDs.Contains(_edge.GUID))
                edgeGUIDs.Add(_edge.GUID);
        }

        public void DisconnectToEdge(string _edgeGUID)
        {
            if (edgeGUIDs.Contains(_edgeGUID))
                edgeGUIDs.Remove(_edgeGUID);
        }

        public void DisconnectToEdge(BaseEdge _edge)
        {
            DisconnectToEdge(_edge.GUID);
        }
        #endregion
    }
}