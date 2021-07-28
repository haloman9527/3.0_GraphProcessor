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
using UnityEngine;

namespace CZToolKit.GraphProcessor
{
    [Serializable]
    public partial class BaseEdge
    {
        /// <summary> 自身GUID </summary>
        [SerializeField] string guid;

        [SerializeField] string inputNodeGUID;
        [SerializeField] string inputFieldName;

        [SerializeField] string outputNodeGUID;
        [SerializeField] string outputFieldName;
    }
}
