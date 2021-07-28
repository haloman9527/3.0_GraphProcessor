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
 *  Github: https://github.com/HalfLobsterMan
 *  Blog: https://www.crosshair.top/
 *
 */
#endregion
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CZToolKit.GraphProcessor
{
    public sealed partial class GroupPanel : IntegratedViewModel
    {
        public static GroupPanel Create(string _title, Vector2 _position)
        {
            GroupPanel group = new GroupPanel();
            group.title = _title;
            group.position.position = _position;
            group.position.size = Vector2.one * 300;
            return group;
        }

        [NonSerialized] BaseGraph owner;
        public BaseGraph Owner
        {
            get { return owner; }
            private set { owner = value; }
        }
        public string Title
        {
            get { return GetPropertyValue<string>(nameof(Title)); }
            set { SetPropertyValue(nameof(Title), value); }
        }
        public Color Color
        {
            get { return GetPropertyValue<Color>(nameof(Color)); }
            set { SetPropertyValue(nameof(Color), value); }
        }
        public Rect Position
        {
            get { return GetPropertyValue<Rect>(nameof(Position)); }
            set { SetPropertyValue(nameof(Position), value); }
        }
        public List<string> InnerNodeGUIDs { get { return innerNodeGUIDs; } }
        //public List<string> InnerStackGUIDs { get { return innerStackGUIDs; } }

        public void Enable(BaseGraph _graph)
        {
            Owner = _graph;
        }

        public override void InitializeBindableProperties()
        {
            this[nameof(Title)] = new BindableProperty<string>(title, v => title = v);
            this[nameof(Color)] = new BindableProperty<Color>(color, v => color = v);
            this[nameof(Position)] = new BindableProperty<Rect>(position, v => position = v);
        }

        public void AddNode(string _guid)
        {
            innerNodeGUIDs.Add(_guid);
        }
        public void RemoveNode(string _guid)
        {
            innerNodeGUIDs.Remove(_guid);
        }
        //public void AddStack(string _guid)
        //{
        //    innerStackGUIDs.Add(_guid);
        //}
        //public void RemoveStack(string _guid)
        //{
        //    innerStackGUIDs.Remove(_guid);
        //}
    }
}