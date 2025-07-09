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

using System;
using System.Collections.Generic;

namespace Atom.GraphProcessor
{
    [ViewModel(typeof(BaseGraph))]
    public partial class BaseGraphProcessor : ViewModel
    {
        /// <summary>
        /// Graph数据
        /// </summary>
        private BaseGraph m_Model;
        
        /// <summary>
        /// Graph数据Type
        /// </summary>
        private Type m_ModelType;
        
        /// <summary>
        /// Graph的操作事件
        /// </summary>
        private GraphEvents m_GraphEvents;

        /// <summary>
        /// 自定义事件
        /// </summary>
        private EventStation<string> m_Events;
        
        /// <summary>
        /// 黑板
        /// </summary>
        private BlackboardProcessor<string> m_Blackboard;

        public BaseGraphProcessor(BaseGraph model)
        {
            m_Model = model;
            m_ModelType = model.GetType();
            m_Model.pan = m_Model.pan == default ? InternalVector2Int.zero : Model.pan;
            m_Model.zoom = m_Model.zoom == 0 ? 1 : Model.zoom;
            m_Model.notes = m_Model.notes == null ? new List<StickyNote>() : Model.notes;

            m_GraphEvents = new GraphEvents();
            m_Events = new EventStation<string>();
            m_Blackboard = new BlackboardProcessor<string>(new Blackboard<string>(), new EventStation<string>());

            BeginInitNodes();
            BeginInitConnections();
            EndInitConnections();
            EndInitNodes();
            InitGroups();
            InitNotes();
        }
        
        public BaseGraph Model
        {
            get { return m_Model; }
        }

        public Type ModelType
        {
            get { return m_ModelType; }
        }

        public InternalVector2Int Pan
        {
            get => Model.pan;
            set => SetFieldValue(ref Model.pan, value, nameof(BaseGraph.pan));
        }

        public float Zoom
        {
            get => Model.zoom;
            set => SetFieldValue(ref Model.zoom, value, nameof(BaseGraph.zoom));
        }

        public GraphEvents GraphEvents
        {
            get { return m_GraphEvents; }
        }

        public EventStation<string> Events
        {
            get { return m_Events; }
        }

        public BlackboardProcessor<string> Blackboard
        {
            get { return m_Blackboard; }
        }
    }
}