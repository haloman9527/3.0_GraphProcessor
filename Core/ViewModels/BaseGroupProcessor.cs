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

using Atom;
using System;
using System.Collections.Generic;

namespace Atom.GraphProcessor
{
    [ViewModel(typeof(Group))]
    public class GroupProcessor : ViewModel, IGraphElementProcessor, IGraphElementProcessor_Scope
    {
        #region Fileds

        private Group model;
        private Type modelType;
        public event Action<BaseNodeProcessor[]> onNodeAdded;
        public event Action<BaseNodeProcessor[]> onNodeRemoved;

        #endregion

        #region Property

        public Group Model => model;
        public Type ModelType => modelType;

        object IGraphElementProcessor.Model => model;

        Type IGraphElementProcessor.ModelType => modelType;

        public int ID => Model.id;

        public IReadOnlyList<int> Nodes => Model.nodes;

        public BaseGraphProcessor Owner { get; internal set; }

        public string GroupName
        {
            get => Model.groupName;
            set => SetFieldValue(ref Model.groupName, value, nameof(Model.groupName));
        }

        public InternalVector2Int Position
        {
            get => Model.position;
            set => SetFieldValue(ref Model.position, value, nameof(Model.position));
        }

        public InternalColor BackgroundColor
        {
            get => Model.backgroundColor;
            set => SetFieldValue(ref Model.backgroundColor, value, nameof(Model.backgroundColor));
        }

        #endregion

        public GroupProcessor(Group model)
        {
            this.model = model;
            this.modelType = model.GetType();
            this.model.position = model.position == default ? InternalVector2Int.zero : model.position;
        }

        internal void NotifyNodeAdded(BaseNodeProcessor[] node)
        {
            onNodeAdded?.Invoke(node);
        }

        internal void NotifyNodeRemoved(BaseNodeProcessor[] node)
        {
            onNodeRemoved?.Invoke(node);
        }
    }
}