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

namespace CZToolKit.GraphProcessor
{
#if UNITY_5_3_OR_NEWER
    [Serializable]
    public class BaseGraph
    {
        [UnityEngine.HideInInspector] public float zoom = 1;
        [UnityEngine.HideInInspector] public InternalVector2Int pan = new InternalVector2Int(0, 0);
        [UnityEngine.HideInInspector] public Dictionary<int, BaseNode> nodes = new Dictionary<int, BaseNode>();
        [UnityEngine.HideInInspector] public List<BaseConnection> connections = new List<BaseConnection>();
        [UnityEngine.HideInInspector] public List<BaseGroup> groups = new List<BaseGroup>();
    }
#else
    [Serializable]
    public class BaseGraph
    {
        public float zoom = 1;
        public InternalVector2Int pan = new InternalVector2Int(0, 0);
        public Dictionary<int, BaseNode> nodes = new Dictionary<int, BaseNode>();
        public List<BaseConnection> connections = new List<BaseConnection>();
        public List<Group> groups = new List<Group>();
    }
#endif
}
