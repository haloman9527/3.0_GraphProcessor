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

        private Group m_Model;
        private Type m_ModelType;

        private BaseGraphProcessor m_Owner;

        #endregion

        #region Property

        public Group Model
        {
            get { return m_Model; }
        }

        object IGraphElementProcessor.Model
        {
            get { return m_Model; }
        }

        public Type ModelType
        {
            get { return m_ModelType; }
        }

        Type IGraphElementProcessor.ModelType
        {
            get { return m_ModelType; }
        }

        public long ID
        {
            get { return Model.id; }
        }

        public IReadOnlyList<long> Nodes
        {
            get { return Model.nodes; }
        }

        public BaseGraphProcessor Owner
        {
            get { return m_Owner; }
            internal set { m_Owner = value; }
        }

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
            this.m_Model = model;
            this.m_ModelType = model.GetType();
            this.m_Model.position = model.position == default ? InternalVector2Int.zero : model.position;
        }

        internal void NotifyNodeAdded(BaseNodeProcessor[] nodes)
        {
            Owner.GraphEvents.Publish(new AddNodesToGroupEventArgs(this, nodes));
        }

        internal void NotifyNodeRemoved(BaseNodeProcessor[] nodes)
        {
            Owner.GraphEvents.Publish(new RemoveNodesFromGroupEventArgs(this, nodes));
        }
    }
}