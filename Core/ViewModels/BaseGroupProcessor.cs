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
 *  Github: https://github.com/haloman9527
 *  Blog: https://www.mindgear.net/
 *
 */

#endregion

using CZToolKit;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CZToolKit.GraphProcessor
{
    [ViewModel(typeof(BaseGroup))]
    public class BaseGroupProcessor : ViewModel, IGraphElementViewModel
    {
        #region Fileds

        public event Action<BaseNodeProcessor> onNodeAdded;
        public event Action<BaseNodeProcessor> onNodeRemoved;

        #endregion

        #region Property

        public BaseGroup Model { get; }
        public Type ModelType { get; }
        public BaseGraphProcessor Owner { get; internal set; }

        public int ID
        {
            get { return Model.id; }
        }
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

        public BaseGroupProcessor(BaseGroup model)
        {
            Model = model;
            ModelType = model.GetType();
            Model.position = Model.position == default ? InternalVector2Int.zero : Model.position;
            this[nameof(BaseGroup.groupName)] = new BindableProperty<string>(() => Model.groupName, v => Model.groupName = v);
            this[nameof(BaseGroup.position)] = new BindableProperty<InternalVector2Int>(() => Model.position, v => Model.position = v);
            this[nameof(BaseGroup.backgroundColor)] = new BindableProperty<InternalColor>(() => Model.backgroundColor, v => Model.backgroundColor = v);
        }

        internal void NotifyNodeAdded(BaseNodeProcessor node)
        {
            onNodeAdded?.Invoke(node);
        }

        internal void NotifyNodeRemoved(BaseNodeProcessor node)
        {
            onNodeRemoved?.Invoke(node);
        }
    }
}