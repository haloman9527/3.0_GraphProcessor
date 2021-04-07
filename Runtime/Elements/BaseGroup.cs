using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GraphProcessor
{
    [Serializable]
    public class BaseGroup
    {
        public string title;
        public Color color = new Color(0, 0, 0, 0.7f);
        public Rect position;
        public Vector2 size;

        public List<string> innerNodeGUIDs = new List<string>();
        public List<string> innerStackGUIDs = new List<string>();

        public BaseGroup() { }

        public BaseGroup(string _title, Vector2 _position)
        {
            this.title = _title;
            this.position.position = _position;
        }

        public virtual void OnCreated()
        {
            size = new Vector2(400, 200);
            position.size = size;
        }
    }
}