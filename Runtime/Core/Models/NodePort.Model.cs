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
using CZToolKit.Core;
using UnityEngine;
using System;
using System.Reflection;
using System.Collections.Generic;

namespace CZToolKit.GraphProcessor
{
    [Serializable]
    public partial class NodePort
    {
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
    }
}