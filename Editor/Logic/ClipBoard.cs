﻿using System;
using System.Collections.Generic;

using UnityObject = UnityEngine.Object;

namespace CZToolKit.GraphProcessor
{
    [Serializable]
    public class ClipBoard
    {
        public static List<UnityObject> objectReferences;

        public List<BaseNode> copiedNodes = new List<BaseNode>();
        public List<BaseEdge> copiedEdges = new List<BaseEdge>();
        public List<BaseStack> copiedStacks = new List<BaseStack>();
        public List<BaseGroup> copiedGroups = new List<BaseGroup>();
    }
}