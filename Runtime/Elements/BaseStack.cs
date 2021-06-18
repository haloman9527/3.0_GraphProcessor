using UnityEngine;
using System.Collections.Generic;
using System;

namespace CZToolKit.GraphProcessor
{
    [Serializable]
    public class BaseStack : IGraphElement
    {
        public static BaseStack CreateStack(Vector2 _position, string _title = "Stack")
        {
            BaseStack stack = new BaseStack();
            stack.title = _title;
            stack.position = _position;
            stack.guid = Guid.NewGuid().ToString();
            return stack;
        }

        [SerializeField]
        string guid;
        public Vector2 position;
        public string title = "New Stack";
        public List<string> nodeGUIDs = new List<string>();

        public string GUID { get { return guid; } }

        protected BaseStack() { }

        public virtual void OnCreated() { }
    }
}