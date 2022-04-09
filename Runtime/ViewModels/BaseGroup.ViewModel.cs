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
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CZToolKit.GraphProcessor
{
    public partial class Group : ViewModel, IGraphElement
    {
        [NonSerialized] BaseGraph owner;

        public event Action<IEnumerable<BaseNode>> onElementsAdded;
        public event Action<IEnumerable<BaseNode>> onElementsRemoved;

        public BaseGraph Owner { get => owner; }


        public string GroupName
        {
            get { return GetPropertyValue<string>(nameof(GroupName)); }
            set { SetPropertyValue(nameof(GroupName), value); }
        }

        public Vector2 Position
        {
            get { return GetPropertyValue<Vector2>(nameof(Position)); }
            set { SetPropertyValue(nameof(Position), value); }
        }

        public IReadOnlyList<string> Nodes
        {
            get => nodes;
        }

        public Group(string groupName)
        {
            this.groupName = groupName;
        }

        internal void Enable(BaseGraph graph)
        {
            owner = graph;
            OnEnabled();
        }

        protected virtual void OnEnabled()
        {
            this[nameof(GroupName)] = new BindableProperty<string>(() => groupName, v => groupName = v);
            this[nameof(Position)] = new BindableProperty<Vector2>(() => position, v => position = v);
        }

        public void AddNodes(IEnumerable<BaseNode> elements)
        {
            var eles = new List<BaseNode>(elements.Where(element => !nodes.Contains(element.GUID) && element.Owner == this.owner));
            foreach (var element in eles)
            {
                nodes.Add(element.GUID);
                foreach (var group in Owner.Groups)
                {
                    if (group == this)
                        continue;
                    group.nodes.Remove(element.GUID);
                }
            }
            onElementsAdded?.Invoke(eles);
        }

        public void RemoveNodes(IEnumerable<BaseNode> elements)
        {
            var eles = new List<BaseNode>(elements.Where(element => nodes.Contains(element.GUID) && element.Owner == this.owner));
            foreach (var element in eles)
            {
                nodes.Remove(element.GUID);
            }
            onElementsRemoved?.Invoke(eles);
        }

        public void AddNode(BaseNode element)
        {
            AddNodes(new BaseNode[] { element });
        }

        public void RemoveNode(BaseNode element)
        {
            RemoveNodes(new BaseNode[] { element });
        }

        public void AddNodesWithoutNotify(IEnumerable<BaseNode> elements)
        {
            elements = elements.Where(element => !nodes.Contains(element.GUID) && element.Owner == this.owner);
            foreach (var element in elements)
            {
                nodes.Add(element.GUID);
            }
        }

        public void RemoveNodesWithoutNotify(IEnumerable<BaseNode> elements)
        {
            elements = elements.Where(element => nodes.Contains(element.GUID) && element.Owner == this.owner);
            foreach (var element in elements)
            {
                nodes.Remove(element.GUID);
            }
        }
    }
}
