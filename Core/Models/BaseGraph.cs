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

namespace Moyo.GraphProcessor
{
    [Serializable]
    public class BaseGraph
    {
        public float zoom = 1;
        public InternalVector2Int pan = new InternalVector2Int(0, 0);
        
        
#if UNITY_5_3_OR_NEWER
        [UnityEngine.SerializeReference] public List<BaseNode> nodes = new List<BaseNode>();
        [UnityEngine.SerializeReference] public List<BaseConnection> connections = new List<BaseConnection>();
        public List<Group> groups = new List<Group>();
        public List<StickyNote> notes = new List<StickyNote>();
#else
        public List<BaseNode> nodes = new List<BaseNode>();
        public List<BaseConnection> connections = new List<BaseConnection>();
        public List<Group> groups = new List<Group>();
        public List<StickyNote> notes = new List<StickyNote>();
#endif
    }
}
