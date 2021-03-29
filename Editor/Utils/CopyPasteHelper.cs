using System;
using System.Collections.Generic;

namespace GraphProcessor
{
    [Serializable]
    public class CopyPasteHelper
    {
        public List<JsonElement> copiedNodes = new List<JsonElement>();
        public List<JsonElement> copiedEdges = new List<JsonElement>();
        public List<JsonElement> copiedGroups = new List<JsonElement>();
        public List<JsonElement> copiedStacks = new List<JsonElement>();
    }
}