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
    public partial class Group : ViewModel
    {
        public event Action<IEnumerable<INode>> onElementsAdded;
        public event Action<IEnumerable<INode>> onElementsRemoved;

        public IGraph Owner { get; internal set; }

        public string GroupName
        {
            get { return GetPropertyValue<string>(nameof(groupName)); }
            set { SetPropertyValue(nameof(groupName), value); }
        }

        public Vector2 Position
        {
            get { return GetPropertyValue<Vector2>(nameof(position)); }
            set { SetPropertyValue(nameof(position), value); }
        }

        public IReadOnlyList<string> Nodes
        {
            get => nodes;
        }

        public Group(string groupName)
        {
            this.groupName = groupName;
        }

        internal void Enable(IGraph graph)
        {
            Owner = graph;
            this[nameof(groupName)] = new BindableProperty<string>(() => groupName, v => groupName = v);
            this[nameof(position)] = new BindableProperty<Vector2>(() => position, v => position = v);
            OnEnabled();
        }

        public virtual void OnEnabled()
        {
        }

        public void AddNodes(IEnumerable<INode> elements)
        {
            var eles = new List<INode>(elements.Where(element => !nodes.Contains(element.GUID) && element.Owner == this.Owner));
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

        public void RemoveNodes(IEnumerable<INode> elements)
        {
            var eles = new List<INode>(elements.Where(element => nodes.Contains(element.GUID) && element.Owner == this.Owner));
            foreach (var element in eles)
            {
                nodes.Remove(element.GUID);
            }
            onElementsRemoved?.Invoke(eles);
        }

        public void AddNode(INode element)
        {
            AddNodes(new INode[] { element });
        }

        public void RemoveNode(INode element)
        {
            RemoveNodes(new INode[] { element });
        }

        public void AddNodesWithoutNotify(IEnumerable<INode> elements)
        {
            elements = elements.Where(element => !nodes.Contains(element.GUID) && element.Owner == this.Owner);
            foreach (var element in elements)
            {
                nodes.Add(element.GUID);
            }
        }

        public void RemoveNodesWithoutNotify(IEnumerable<INode> elements)
        {
            elements = elements.Where(element => nodes.Contains(element.GUID) && element.Owner == this.Owner);
            foreach (var element in elements)
            {
                nodes.Remove(element.GUID);
            }
        }
    }
}
