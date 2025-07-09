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
    public partial class BaseGraphProcessor
    {
        #region Fields

        private Dictionary<long, StickyNoteProcessor> m_Notes;

        #endregion

        #region Properties

        public IReadOnlyDictionary<long, StickyNoteProcessor> Notes
        {
            get { return m_Notes; }
        }

        #endregion

        private void InitNotes()
        {
            this.m_Notes = new Dictionary<long, StickyNoteProcessor>(System.Math.Min(Model.connections.Count, 4));
            foreach (var note in m_Model.notes)
            {
                m_Notes.Add(note.id, (StickyNoteProcessor)ViewModelFactory.ProduceViewModel(note));
            }
        }

        #region API

        public void AddNote(string title, string content, InternalVector2Int position)
        {
            var note = new StickyNote();
            note.id = GraphProcessorUtil.GenerateId();
            note.position = position;
            note.title = title;
            note.content = content;
            AddNote(ViewModelFactory.ProduceViewModel(note) as StickyNoteProcessor);
        }

        public void AddNote(StickyNoteProcessor note)
        {
            m_Notes.Add(note.ID, note);
            m_Model.notes.Add(note.Model);
            m_GraphEvents.Publish(new AddNoteEventArgs(note));
        }

        public void RemoveNote(long id)
        {
            if (!m_Notes.TryGetValue(id, out var note))
                return;
            m_Notes.Remove(note.ID);
            m_Model.notes.Remove(note.Model);
            m_GraphEvents.Publish(new RemoveNoteEventArgs(note));
        }

        #endregion
    }
}