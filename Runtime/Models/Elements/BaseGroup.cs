using System;
using System.Collections.Generic;
using UnityEngine;

namespace CZToolKit.GraphProcessor
{
    [Serializable]
    public class BaseGroup : BaseGraphElement
    {
        public static BaseGroup Create(string _title, Vector2 _position)
        {
            BaseGroup group = new BaseGroup();
            group.title = _title;
            group.position.position = _position;
            group.position.size = Vector2.one * 300;
            return group;
        }

        #region Model
        [SerializeField] string title;
        [SerializeField] Color color = new Color(0, 0, 0, 0.7f);
        [SerializeField] Rect position;

        [SerializeField] List<string> innerNodeGUIDs = new List<string>();
        [SerializeField] List<string> innerStackGUIDs = new List<string>();
        #endregion

        #region ViewModel
        [NonSerialized] BaseGraph owner;
        public BaseGraph Owner
        {
            get { return owner; }
            private set { owner = value; }
        }
        public string Title
        {
            get { return GetPropertyValue<string>(nameof(title)); }
            set { SetPropertyValue(nameof(title), value); }
        }
        public Color Color
        {
            get { return GetPropertyValue<Color>(nameof(color)); }
            set { SetPropertyValue(nameof(color), value); }
        }
        public Rect Position
        {
            get { return GetPropertyValue<Rect>(nameof(position)); }
            set { SetPropertyValue(nameof(position), value); }
        }
        public List<string> InnerNodeGUIDs { get { return innerNodeGUIDs; } }
        public List<string> InnerStackGUIDs { get { return innerStackGUIDs; } }

        public void Enable(BaseGraph _graph)
        {
            Owner = _graph;
        }

        public override void InitializeBindableProperties()
        {
            SetBindableProperty(nameof(title), new BindableProperty<string>(title, v => title = v));
            SetBindableProperty(nameof(color), new BindableProperty<Color>(color, v => color = v));
            SetBindableProperty(nameof(position), new BindableProperty<Rect>(position, v => position = v));
        }

        public void AddNode(string _guid)
        {
            innerNodeGUIDs.Add(_guid);
        }
        public void RemoveNode(string _guid)
        {
            innerNodeGUIDs.Remove(_guid);
        }
        public void AddStack(string _guid)
        {
            innerStackGUIDs.Add(_guid);
        }
        public void RemoveStack(string _guid)
        {
            innerStackGUIDs.Remove(_guid);
        }
        #endregion
    }
}