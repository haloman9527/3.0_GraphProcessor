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
    [NodeMenuItem("Parameter", showInList = false)]
    public partial class ParameterNode : BaseNode
    {
        [Port(PortDirection.Output)]
        [InspectorName("Value")]
        [SerializeField] object output;

        [HideInInspector]
        [SerializeField] string name;
    }
}
