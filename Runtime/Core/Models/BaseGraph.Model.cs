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
using CZToolKit.Core.Blackboards;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CZToolKit.GraphProcessor
{
    [Serializable]
    public partial class BaseGraph
    {
        public static readonly Vector2 DefaultBlackboardSize = new Vector2(150, 200);

        [SerializeField] Vector3 position = Vector3.zero;
        [SerializeField] Vector3 scale = Vector3.one;
        [SerializeField] bool blackboardVisible = true;
        [SerializeField] Rect blackboardPosition = new Rect(Vector2.zero, DefaultBlackboardSize);

        [SerializeField] Dictionary<string, BaseNode> nodes = new Dictionary<string, BaseNode>();
        [SerializeField] Dictionary<string, BaseEdge> edges = new Dictionary<string, BaseEdge>();
        //[SerializeField] Dictionary<string, StackPanel> stacks = new Dictionary<string, StackPanel>();
        [SerializeField] List<GroupPanel> groups = new List<GroupPanel>();

        [SerializeField] CZBlackboardWithGUID blackboard = new CZBlackboardWithGUID();
    }
}
