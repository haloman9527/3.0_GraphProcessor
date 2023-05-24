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

using CZToolKit.Common.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CZToolKit.GraphProcessor
{
    [ViewModel(typeof(BaseGroup))]
    public class BaseGroupVM : ViewModel, IGraphElementViewModel
    {
        #region Fileds

        public event Action<IEnumerable<BaseNodeVM>> onNodesAdded;
        public event Action<IEnumerable<BaseNodeVM>> onNodesRemoved;

        #endregion

        #region Property

        public BaseGroup Model { get; }
        public Type ModelType { get; }
        public BaseGraphVM Owner { get; internal set; }

        public string GroupName
        {
            get { return GetPropertyValue<string>(nameof(Model.groupName)); }
            set { SetPropertyValue(nameof(Model.groupName), value); }
        }

        public InternalVector2Int Position
        {
            get { return GetPropertyValue<InternalVector2Int>(nameof(Model.position)); }
            set { SetPropertyValue(nameof(Model.position), value); }
        }

        public InternalColor BackgroundColor
        {
            get { return GetPropertyValue<InternalColor>(nameof(Model.backgroundColor)); }
            set { SetPropertyValue(nameof(Model.backgroundColor), value); }
        }

        public IReadOnlyList<int> Nodes
        {
            get { return Model.nodes; }
        }

        #endregion

        public BaseGroupVM(BaseGroup model)
        {
            Model = model;
            ModelType = model.GetType();
            Model.position = Model.position == default ? InternalVector2Int.zero : Model.position;
            this[nameof(BaseGroup.groupName)] = new BindableProperty<string>(() => Model.groupName, v => Model.groupName = v);
            this[nameof(BaseGroup.position)] = new BindableProperty<InternalVector2Int>(() => Model.position, v => Model.position = v);
            this[nameof(BaseGroup.backgroundColor)] = new BindableProperty<InternalColor>(() => Model.backgroundColor, v => Model.backgroundColor = v);
        }

        #region Private
        
        private void InternalAddNodes(BaseNodeVM[] nodes, bool withoutNotify)
        {
            var tempNodes = nodes.Where(element => !Model.nodes.Contains(element.ID) && element.Owner == this.Owner).ToArray();
            foreach (var node in tempNodes)
            {
                if (Model.nodes.Contains(node.ID))
                    continue;
                if (node.Owner != Owner)
                    continue;
                foreach (var group in Owner.Groups)
                {
                    group.Model.nodes.Remove(node.ID);
                }

                Model.nodes.Add(node.ID);
            }

            if (withoutNotify)
                return;

            onNodesAdded?.Invoke(tempNodes);
        }

        private void InternalRemoveNodes(BaseNodeVM[] nodes, bool withoutNotify)
        {
            var tempNodes = nodes.Where(element => Model.nodes.Contains(element.ID) && element.Owner == this.Owner).ToArray();
            foreach (var node in tempNodes)
            {
                if (!Model.nodes.Contains(node.ID))
                    continue;
                if (node.Owner != Owner)
                    continue;
                Model.nodes.Remove(node.ID);
            }

            if (withoutNotify)
                return;

            onNodesRemoved?.Invoke(tempNodes);
        }
        
        #endregion
        
        #region API

        public void AddNodes(BaseNodeVM[] nodes)
        {
            InternalAddNodes(nodes, true);
        }

        public void RemoveNodes(BaseNodeVM[] nodes)
        {
            InternalAddNodes(nodes, true);
        }

        public void AddNode(BaseNodeVM element)
        {
            InternalAddNodes(new BaseNodeVM[] { element }, true);
        }

        public void RemoveNode(BaseNodeVM element)
        {
            InternalRemoveNodes(new BaseNodeVM[] { element }, true);
        }

        public void AddNodesWithoutNotify(BaseNodeVM[] elements)
        {
            InternalAddNodes(elements, false);
        }

        public void RemoveNodesWithoutNotify(BaseNodeVM[] elements)
        {
            InternalAddNodes(elements, false);
        }

        #endregion
    }
}