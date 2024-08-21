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
 *  Blog: https://www.haloman.net/
 *
 */

#endregion

using CZToolKit;
using System;
using System.Collections.Generic;

namespace CZToolKit.GraphProcessor
{
    [ViewModel(typeof(BaseGroup))]
    public class BaseGroupProcessor : ViewModel, IGraphScopeViewModel
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
            get { return GetField<string>(nameof(Model.groupName)); }
            set { SetField(nameof(Model.groupName), value); }
        }

        public InternalVector2Int Position
        {
            get { return GetField<InternalVector2Int>(nameof(Model.position)); }
            set { SetField(nameof(Model.position), value); }
        }

        public InternalColor BackgroundColor
        {
            get { return GetField<InternalColor>(nameof(Model.backgroundColor)); }
            set { SetField(nameof(Model.backgroundColor), value); }
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

            this.RegisterField(nameof(BaseGroup.groupName), () => ref model.groupName);
            this.RegisterField(nameof(BaseGroup.position), () => ref model.position);
            this.RegisterField(nameof(BaseGroup.backgroundColor), () => ref model.backgroundColor);
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