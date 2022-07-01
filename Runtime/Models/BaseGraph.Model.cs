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
    public partial class BaseGraph
    {
        [UnityEngine.HideInInspector] public InternalVector3 pan = new InternalVector3(0, 0, 0);
        [UnityEngine.HideInInspector] public InternalVector3 zoom = new InternalVector3(0, 0, 0);
        [UnityEngine.HideInInspector] public Dictionary<string, BaseNode> nodes = new Dictionary<string, BaseNode>();
        [UnityEngine.HideInInspector] public List<BaseConnection> connections = new List<BaseConnection>();
        [UnityEngine.HideInInspector] public List<Group> groups = new List<Group>();
    }
#else
    [Serializable]
    public partial class BaseGraph
    {
        public InternalVector3 pan = new InternalVector3(0, 0, 0);
        public InternalVector3 zoom = new InternalVector3(0, 0, 0);
        public Dictionary<string, BaseNode> nodes = new Dictionary<string, BaseNode>();
        public List<BaseConnection> connections = new List<BaseConnection>();
        public List<Group> groups = new List<Group>();
    }
#endif
}
