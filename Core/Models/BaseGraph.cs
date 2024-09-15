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

namespace CZToolKit.GraphProcessor
{
    [Serializable]
    public class BaseGraph
    {
        public float zoom = 1;
        public InternalVector2Int pan = new InternalVector2Int(0, 0);
        public Dictionary<int, BaseNode> nodes = new Dictionary<int, BaseNode>();
        public List<BaseConnection> connections = new List<BaseConnection>();
        public List<BaseGroup> groups = new List<BaseGroup>();
        public Dictionary<int, StickyNote> notes = new Dictionary<int, StickyNote>();
    }
}
