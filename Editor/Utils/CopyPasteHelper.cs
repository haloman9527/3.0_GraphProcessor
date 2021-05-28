using System;
using System.Collections.Generic;

using UnityObject = UnityEngine.Object;

namespace CZToolKit.GraphProcessor
{
    [Serializable]
    public class CopyPasteHelper
    {
        public static List<UnityObject> objectReferences;

        public List<BaseNode> copiedNodes = new List<BaseNode>();
        public List<SerializableEdge> copiedEdges = new List<SerializableEdge>();
        public List<BaseStackNode> copiedStacks = new List<BaseStackNode>();
        public List<BaseGroup> copiedGroups = new List<BaseGroup>();
    }
}