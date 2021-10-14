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
        [SerializeField] protected Vector3 panOffset = Vector3.zero;
        [SerializeField] protected Vector3 scale = Vector3.one;

        [SerializeField] protected Dictionary<string, BaseNode> nodes = new Dictionary<string, BaseNode>();
        [SerializeField] protected List<BaseConnection> connections = new List<BaseConnection>();
    }
}
