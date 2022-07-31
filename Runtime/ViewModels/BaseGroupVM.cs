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

namespace CZToolKit.GraphProcessor
{
    [ViewModel(typeof(BaseGroup))]
    public class BaseGroupVM : ViewModel
    {
        public event Action<IEnumerable<BaseNodeVM>> onNodesAdded;
        public event Action<IEnumerable<BaseNodeVM>> onNodesRemoved;

        #region Property
        public BaseGroup Model
        {
            get;
        }
        public Type ModelType
        {
            get;
        }
        public BaseGraphVM Owner
        {
            get;
            private set;
        }
        public string GroupName
        {
            get { return GetPropertyValue<string>(nameof(Model.groupName)); }
            set { SetPropertyValue(nameof(Model.groupName), value); }
        }
        public InternalVector2 Position
        {
            get { return GetPropertyValue<InternalVector2>(nameof(Model.position)); }
            set { SetPropertyValue(nameof(Model.position), value); }
        }
        public InternalColor BackgroundColor
        {
            get { return GetPropertyValue<InternalColor>(nameof(Model.backgroundColor)); }
            set { SetPropertyValue(nameof(Model.backgroundColor), value); }
        }
        public IReadOnlyList<string> Nodes
        {
            get { return Model.nodes; }
        }
        #endregion

        public BaseGroupVM(BaseGroup model)
        {
            Model = model;
            ModelType = model.GetType();
            Model.position = Model.position == default ? InternalVector2.zero : Model.position;
            this[nameof(BaseGroup.groupName)] = new BindableProperty<string>(() => Model.groupName, v => Model.groupName = v);
            this[nameof(BaseGroup.position)] = new BindableProperty<InternalVector2>(() => Model.position, v => Model.position = v);
            this[nameof(BaseGroup.backgroundColor)] = new BindableProperty<InternalColor>(() => Model.backgroundColor, v => Model.backgroundColor = v);
        }

        internal void Enable(BaseGraphVM graph)
        {
            Owner = graph;
            OnEnabled();
        }

        public void AddNodes(IEnumerable<BaseNodeVM> nodes)
        {
            var tempNodes = nodes.Where(element => !Model.nodes.Contains(element.GUID) && element.Owner == this.Owner).ToArray();
            foreach (var element in tempNodes)
            {
                foreach (var group in Owner.Groups)
                {
                    group.Model.nodes.Remove(element.GUID);
                }
                Model.nodes.Add(element.GUID);
            }
            onNodesAdded?.Invoke(tempNodes);
        }

        public void RemoveNodes(IEnumerable<BaseNodeVM> nodes)
        {
            var tempNodes = nodes.Where(element => Model.nodes.Contains(element.GUID) && element.Owner == Owner).ToArray();
            foreach (var node in tempNodes)
            {
                Model.nodes.Remove(node.GUID);
            }
            onNodesRemoved?.Invoke(nodes);
        }

        public void AddNode(BaseNodeVM element)
        {
            AddNodes(new BaseNodeVM[] { element });
        }

        public void RemoveNode(BaseNodeVM element)
        {
            RemoveNodes(new BaseNodeVM[] { element });
        }

        public void AddNodesWithoutNotify(IEnumerable<BaseNodeVM> elements)
        {
            elements = elements.Where(element => !Model.nodes.Contains(element.GUID) && element.Owner == this.Owner);
            foreach (var element in elements)
            {
                Model.nodes.Add(element.GUID);
            }
        }

        public void RemoveNodesWithoutNotify(IEnumerable<BaseNodeVM> elements)
        {
            elements = elements.Where(element => Model.nodes.Contains(element.GUID) && element.Owner == this.Owner);
            foreach (var element in elements)
            {
                Model.nodes.Remove(element.GUID);
            }
        }

        #region Overrides
        public virtual void OnEnabled() { }

        #endregion
    }
}
