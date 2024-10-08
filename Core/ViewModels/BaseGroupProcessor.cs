﻿#region 注 释

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
    public class BaseGroupProcessor : ViewModel, IGraphElementProcessor, IGraphElementProcessor_Scope
    {
        #region Fileds

        public event Action<BaseNodeProcessor> onNodeAdded;
        public event Action<BaseNodeProcessor> onNodeRemoved;

        #endregion

        #region Property

        public BaseGroup Model { get; }
        public Type ModelType { get; }
        public BaseGraphProcessor Owner { get; internal set; }

        public int ID => Model.id;

        public IReadOnlyList<int> Nodes => Model.nodes;

        public string GroupName
        {
            get => GetPropertyValue<string>(nameof(Model.groupName));
            set => SetPropertyValue(nameof(Model.groupName), value);
        }

        public InternalVector2Int Position
        {
            get => GetPropertyValue<InternalVector2Int>(nameof(Model.position));
            set => SetPropertyValue(nameof(Model.position), value);
        }

        public InternalColor BackgroundColor
        {
            get => GetPropertyValue<InternalColor>(nameof(Model.backgroundColor));
            set => SetPropertyValue(nameof(Model.backgroundColor), value);
        }

        #endregion

        public BaseGroupProcessor(BaseGroup model)
        {
            Model = model;
            ModelType = model.GetType();
            Model.position = Model.position == default ? InternalVector2Int.zero : Model.position;

            this.RegisterProperty(nameof(BaseGroup.groupName), () => ref model.groupName);
            this.RegisterProperty(nameof(BaseGroup.position), () => ref model.position);
            this.RegisterProperty(nameof(BaseGroup.backgroundColor), () => ref model.backgroundColor);
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