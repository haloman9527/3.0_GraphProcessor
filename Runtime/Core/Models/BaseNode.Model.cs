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
    public abstract partial class BaseNode
    {
        /// <summary> 唯一标识 </summary>
        [SerializeField] string guid;
        /// <summary> 位置坐标 </summary>
        [SerializeField] Vector2 position;
        /// <summary> 展开状态 </summary>
        [SerializeField] bool expanded = true;
        /// <summary> 锁定状态 </summary>
        [SerializeField] bool locked = false;

        [SerializeField] Dictionary<string, NodePort> ports = new Dictionary<string, NodePort>();
    }
}
