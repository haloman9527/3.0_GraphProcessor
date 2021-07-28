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
using UnityEngine;
using System.Collections.Generic;
using System;

namespace CZToolKit.GraphProcessor
{
    public partial class StackPanel : IntegratedViewModel
    {
        public static StackPanel CreateStack(Vector2 _position, string _title = "Stack")
        {
            StackPanel stack = new StackPanel();
            stack.title = _title;
            stack.position = _position;
            stack.guid = Guid.NewGuid().ToString();
            return stack;
        }

        #region ��̬����
        /// <summary> �ӿڼ����Լ�� </summary>
        public static bool IsCompatible(NodePort _port1, NodePort _port2)
        {
            if (_port1 == null || _port2 == null) return false;

            if (_port1.Owner == _port2.Owner)
                return false;

            if (_port1.Direction == _port2.Direction)
                return false;

            if (_port1.TypeConstraint == PortTypeConstraint.None || _port2.TypeConstraint == PortTypeConstraint.None)
                return true;

            bool Compatible(NodePort portA, NodePort portB)
            {
                if (portA.TypeConstraint == PortTypeConstraint.Inherited && portA.DisplayType.IsAssignableFrom(portB.DisplayType)) return true;
                if (portA.TypeConstraint == PortTypeConstraint.Strict && portA.DisplayType == portB.DisplayType) return true;
                return false;
            }

            return Compatible(_port1, _port2) && Compatible(_port2, _port1);
        }
        #endregion

        public event Action<string> onNodeAdded;
        public event Action<int, string> onNodeInserted;
        public event Action<string> onNodeRemoved;

        [NonSerialized] BaseGraph owner;
        public BaseGraph Owner
        {
            get { return owner; }
            private set { owner = value; }
        }
        public string GUID { get { return guid; } }
        public string Title
        {
            get { return title; }
            set { SetPropertyValue(nameof(Title), value); }
        }
        public Vector2 Position
        {
            get { return position; }
            set { SetPropertyValue(nameof(Position), value); }
        }
        public IReadOnlyList<string> NodeGUIDs
        {
            get { return nodeGUIDs; }
        }

        public void Enable(BaseGraph _graph)
        {
            Owner = _graph;
        }

        public override void InitializeBindableProperties()
        {
            this[nameof(Title)] = new BindableProperty<string>(title, v => title = v);
            this[nameof(Position)] = new BindableProperty<Vector2>(position, v => position = v);
        }

        public void Add(string _guid)
        {
            if (!nodeGUIDs.Contains(_guid))
            {
                nodeGUIDs.Add(_guid);
                onNodeAdded?.Invoke(_guid);
            }
        }

        public void Remove(string _guid)
        {
            if (nodeGUIDs.Contains(_guid))
            {
                nodeGUIDs.Remove(_guid);
                onNodeRemoved?.Invoke(_guid);
            }
        }

        public void Insert(int _index, string _guid)
        {
            nodeGUIDs.Insert(_index, _guid);
        }

        public int FindIndex(Predicate<string> p)
        {
            return nodeGUIDs.FindIndex(p);
        }
    }
}