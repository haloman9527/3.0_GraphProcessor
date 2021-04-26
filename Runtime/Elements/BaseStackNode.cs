using UnityEngine;
using System.Collections.Generic;
using System;

namespace CZToolKit.GraphProcessor
{
    [Serializable]
    public class BaseStackNode
    {
        [SerializeField] string guid;
        public Vector2 position;
        public string title = "New Stack";
        public List<string> nodeGUIDs = new List<string>();

        public string GUID { get { return guid; } }

        public BaseStackNode(Vector2 position, string title = "Stack", bool acceptDrop = true, bool acceptNewNode = true)
        {
            this.position = position;
            this.title = title;
        }

        public void OnCreated()
        {
            guid = Guid.NewGuid().ToString();
        }
    }
}