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
    public abstract partial class BaseGraph
    {
        [SerializeField] Vector3 pan = Vector3.zero;
        [SerializeField] Vector3 zoom = Vector3.one;

        [SerializeField] Dictionary<string, BaseNode> nodes = new Dictionary<string, BaseNode>();
        [SerializeField] List<BaseConnection> connections = new List<BaseConnection>();
        [SerializeField] List<Group> groups = new List<Group>();
    }
}
