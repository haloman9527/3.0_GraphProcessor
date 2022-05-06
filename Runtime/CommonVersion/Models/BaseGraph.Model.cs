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
using UnityEngine;

namespace CZToolKit.GraphProcessor
{
    [Serializable]
    public partial class BaseGraph
    {
        [SerializeField] [HideInInspector] internal Vector3 pan = Vector3.zero;
        [SerializeField] [HideInInspector] internal Vector3 zoom = Vector3.one;

        [SerializeField] [HideInInspector] internal Dictionary<string, BaseNode> nodes = new Dictionary<string, BaseNode>();
        [SerializeField] [HideInInspector] internal List<BaseConnection> connections = new List<BaseConnection>();
        [SerializeField] [HideInInspector] internal List<Group> groups = new List<Group>();
    }
}
