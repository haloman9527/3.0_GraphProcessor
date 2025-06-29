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

        private Dictionary<long, StickyNoteProcessor> notes;

        public event Action<StickyNoteProcessor> OnNoteAdded;
        public event Action<StickyNoteProcessor> OnNoteRemoved;

        #endregion

        #region Properties

        public IReadOnlyDictionary<long, StickyNoteProcessor> Notes => notes;

        #endregion

        private void InitNotes()
        {
            this.notes = new Dictionary<long, StickyNoteProcessor>(System.Math.Min(Model.connections.Count, 4));
            foreach (var note in model.notes)
            {
                notes.Add(note.id, (StickyNoteProcessor)ViewModelFactory.ProduceViewModel(note));
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
            var noteVm = ViewModelFactory.ProduceViewModel(note) as StickyNoteProcessor;

            AddNote(noteVm);
        }

        public void AddNote(StickyNoteProcessor note)
        {
            notes.Add(note.ID, note);
            Model.notes.Add(note.Model);
            OnNoteAdded?.Invoke(note);
        }

        public void RemoveNote(long id)
        {
            if (!notes.TryGetValue(id, out var note))
                return;
            notes.Remove(note.ID);
            Model.notes.Remove(note.Model);
            OnNoteRemoved?.Invoke(note);
        }

        #endregion
    }
}