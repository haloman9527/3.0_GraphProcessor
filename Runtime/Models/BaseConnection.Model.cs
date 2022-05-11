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
    public partial class BaseConnection
    {
        [SerializeField] [HideInInspector] internal string from;
        [SerializeField] [HideInInspector] internal string fromPortName;
        [SerializeField] [HideInInspector] internal string to;
        [SerializeField] [HideInInspector] internal string toPortName;
    }
}
