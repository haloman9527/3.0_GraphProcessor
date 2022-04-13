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
        [SerializeField] internal string groupName;
        [SerializeField] internal Vector2 position;
        [SerializeField] internal List<string> nodes = new List<string>();
    }
}