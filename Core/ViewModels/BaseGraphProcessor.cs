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
using Atom;

namespace Atom.GraphProcessor
{
    [ViewModel(typeof(BaseGraph))]
    public partial class BaseGraphProcessor : ViewModel
    {
        #region Fields

        private BaseGraph model;
        private Type modelType;

        #endregion

        #region Properties

        public BaseGraph Model => model;

        public Type ModelType => modelType;

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

        public EventStation<string> Events { get; }

        public BlackboardProcessor<string> Blackboard { get; }

        #endregion

        public BaseGraphProcessor(BaseGraph model)
        {
            this.model = model;
            this.modelType = model.GetType();
            this.model.pan = Model.pan == default ? InternalVector2Int.zero : Model.pan;
            this.model.zoom = Model.zoom == 0 ? 1 : Model.zoom;
            this.model.notes = Model.notes == null ? new List<StickyNote>() : Model.notes;

            this.Events = new EventStation<string>();
            this.Blackboard = new BlackboardProcessor<string>(new Blackboard<string>(), Events);

            BeginInitNodes();
            BeginInitConnections();
            EndInitConnections();
            EndInitNodes();
            InitGroups();
            InitNotes();
        }
    }
}