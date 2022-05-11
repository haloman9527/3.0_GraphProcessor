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
using System.Collections.Generic;
using UnityEngine;

namespace CZToolKit.GraphProcessor
{
    public partial class Group
    {
        [SerializeField] [HideInInspector] internal string groupName;
        [SerializeField] [HideInInspector] internal Vector2 position;
        [SerializeField] [HideInInspector] internal List<string> nodes = new List<string>();
    }
}