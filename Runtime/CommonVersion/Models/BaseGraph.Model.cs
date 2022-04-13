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
        [SerializeField] internal Vector3 pan = Vector3.zero;
        [SerializeField] internal Vector3 zoom = Vector3.one;

        [SerializeField] internal Dictionary<string, BaseNode> nodes = new Dictionary<string, BaseNode>();
        [SerializeField] internal List<BaseConnection> connections = new List<BaseConnection>();
        [SerializeField] internal List<Group> groups = new List<Group>();
    }
}
